using Journey.Database;
using Journey.EventSourcing;
using Journey.Messaging;
using Journey.Messaging.Logging;
using Journey.Messaging.Logging.Metadata;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils.SystemDateTime;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;

namespace Journey.Worker
{
    public class WorkerRole : IWorkerRole
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly IUnityContainer container;
        private readonly List<IMessageProcessor> processors;
        private static IWorkerRoleTracer _tracer;        
        
        /// <summary>
        /// Acepta aparte del dominio un tracer, que puede ser de consola o web, hasta el momento.
        /// </summary>
        public WorkerRole(IDomainWorkerRegistry domainRegistry, IWorkerRoleTracer tracer)
        {
            _tracer = tracer;
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
            this.cancellationTokenSource = new CancellationTokenSource();
            this.container = this.CreateContainer(domainRegistry);
            this.processors = this.container.ResolveAll<IMessageProcessor>().ToList();
        }

        public void Start()
        {
            this.processors.ForEach(p => p.Start());
            this.container.Resolve<IWorkerRoleTracer>().Notify("=== Worker Started ===");
        }

        public void Stop()
        {
            this.cancellationTokenSource.Cancel();

            this.processors.ForEach(p => p.Stop());
            this.container.Resolve<IWorkerRoleTracer>().Notify("=== Worker Stopped ===");
        }

        private IUnityContainer CreateContainer(IDomainWorkerRegistry domainRegistry)
        {
            var container = new UnityContainer();

            container.RegisterInstance<IDomainWorkerRegistry>(domainRegistry);

            // Infrastructure
            container.RegisterInstance<ISystemDateTime>(new LocalDateTime());
            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());
            container.RegisterInstance<IWorkerRoleTracer>(_tracer);

            var config = container.Resolve<IDomainWorkerRegistry>().Config;

            var serializer = container.Resolve<ITextSerializer>();
            var metadata = container.Resolve<IMetadataProvider>();
            var tracer = container.Resolve<IWorkerRoleTracer>();
            var dateTime = container.Resolve<ISystemDateTime>();

            var commandBus = new CommandBus(
                new MessageSender(System.Data.Entity.Database.DefaultConnectionFactory, config.EventStoreConnectionString, config.CommandBusTableName), serializer);

            var eventBus = new EventBus(
                new MessageSender(System.Data.Entity.Database.DefaultConnectionFactory, config.EventStoreConnectionString, config.EventBusTableName), serializer);

            var commandProcessor = new CommandProcessor(
                new MessageReceiver(System.Data.Entity.Database.DefaultConnectionFactory, config.EventStoreConnectionString, config.CommandBusTableName, config.BusPollDelay, config.NumberOfProcessorsThreads, dateTime), serializer, tracer, new CommandBusTransientFaultDetector(config.EventStoreConnectionString));

            var liveEventProcessor = new EventProcessor(
                new MessageReceiver(System.Data.Entity.Database.DefaultConnectionFactory, config.EventStoreConnectionString, config.EventBusTableName, config.BusPollDelay, config.NumberOfProcessorsThreads, dateTime), serializer, tracer);

            var inMemorySnapshotCache = new InMemoryRollingSnapshot("EventStoreCache");

            container.RegisterInstance<IInMemoryRollingSnapshotProvider>(inMemorySnapshotCache);
            container.RegisterInstance<ICommandBus>(commandBus);
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<ICommandHandlerRegistry>(commandProcessor);
            container.RegisterInstance<IMessageProcessor>("CommandProcessor", commandProcessor);
            container.RegisterInstance<IEventHandlerRegistry>(liveEventProcessor);
            container.RegisterInstance<IMessageProcessor>("EventProcessor", liveEventProcessor);

            var indentedSerializer = new IndentedJsonTextSerializer();
            // Event log database and handler
            this.RegisterMessageLogger(container, indentedSerializer, metadata, liveEventProcessor, config.MessageLogConnectionString, dateTime, tracer);

            // Event Store
            this.RegisterEventStore(container, config.EventStoreConnectionString);

            // Bounded Context Registration
            if (domainRegistry.RegistrationList.Any())
                foreach (var registrationAction in domainRegistry.RegistrationList)
                    registrationAction(container, liveEventProcessor);

            // Handlers
            this.RegisterCommandHandlers(container);
            this.RegisterAditionalEventHandlers(container, liveEventProcessor);

            return container;
        }

        private void RegisterMessageLogger(UnityContainer container, ITextSerializer serializer, IMetadataProvider metadata, EventProcessor eventProcessor, string connectionString, ISystemDateTime dateTime, IWorkerRoleTracer tracer)
        {
            //Database.SetInitializer<MessageLogDbContext>(null);
            container.RegisterType<IMessageAuditLog, MessageLog>(new InjectionConstructor(connectionString, serializer, metadata, dateTime, tracer));
            container.RegisterType<IEventHandler, MessageLogHandler>("MessageLogHandler");
            container.RegisterType<ICommandHandler, MessageLogHandler>("MessageLogHandler");
            eventProcessor.Register(container.Resolve<MessageLogHandler>());
        }

        private void RegisterEventStore(IUnityContainer container, string connectionString)
        {
            //Database.SetInitializer<EventStoreDbContext>(null);
            container.RegisterType<EventStoreDbContext>(new TransientLifetimeManager(), new InjectionConstructor(connectionString));
            container.RegisterType(typeof(IEventStore<>), typeof(EventStore<>), new ContainerControlledLifetimeManager());
        }

        private void RegisterCommandHandlers(IUnityContainer container)
        {
            var commandHandlerRegistry = container.Resolve<ICommandHandlerRegistry>();

            foreach (var commandHandler in container.ResolveAll<ICommandHandler>())
                commandHandlerRegistry.Register(commandHandler);
        }

        private void RegisterAditionalEventHandlers(IUnityContainer container, IEventHandlerRegistry eventProcessor)
        {
            // Register aditional Event Handlers here that does not belong to a Domain Bounded Context

            // Example: Logger
            //eventProcessor.Register(container.Resolve<MessageLogHandler>());
        }

        public void Dispose()
        {
            this.container.Dispose();
            this.cancellationTokenSource.Dispose();
        }

        public IWorkerRoleTracer Tracer
        {
            get { return _tracer; }
        }

        public void RebuildReadModel()
        {
            throw new System.NotImplementedException();
        }
    }
}
