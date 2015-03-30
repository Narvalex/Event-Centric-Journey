using Journey.Database;
using Journey.EventSourcing;
using Journey.Messaging;
using Journey.Messaging.Logging;
using Journey.Messaging.Logging.Metadata;
using Journey.Messaging.Processing;
using Journey.Serialization;
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
        

        public WorkerRole(IDomainContainer domainContainer)
        {
            _tracer = new WebWorkerTracer();
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
            this.cancellationTokenSource = new CancellationTokenSource();
            this.container = this.CreateContainer(domainContainer);
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

        private IUnityContainer CreateContainer(IDomainContainer domainContainer)
        {
            var container = new UnityContainer();

            container.RegisterInstance<IDomainContainer>(domainContainer);

            // Infrastructure
            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());
            container.RegisterInstance<IWorkerRoleTracer>(_tracer);

            var config = container.Resolve<IDomainContainer>().WorkerRoleConfig;

            var serializer = container.Resolve<ITextSerializer>();
            var metadata = container.Resolve<IMetadataProvider>();
            var tracer = container.Resolve<IWorkerRoleTracer>();

            var commandBus = new CommandBus(new MessageSender(System.Data.Entity.Database.DefaultConnectionFactory, config.EventStoreConnectionString, "Bus.Commands"), serializer);
            var eventBus = new EventBus(new MessageSender(System.Data.Entity.Database.DefaultConnectionFactory, config.EventStoreConnectionString, "Bus.Events"), serializer);

            var commandProcessor = new CommandProcessor(new MessageReceiver(System.Data.Entity.Database.DefaultConnectionFactory, config.EventStoreConnectionString, "Bus.Commands", config.BusPollDelay, config.NumberOfProcessorsThreads), serializer, tracer, new BusTransientFaultDetector(config.EventStoreConnectionString));
            var eventProcessor = new EventProcessor(new MessageReceiver(System.Data.Entity.Database.DefaultConnectionFactory, config.EventStoreConnectionString, "Bus.Events", config.BusPollDelay, config.NumberOfProcessorsThreads), serializer, tracer);

            var inMemorySnapshotCache = new InMemorySnapshotCache("EventStoreCache");

            container.RegisterInstance<ISnapshotCache>(inMemorySnapshotCache);
            container.RegisterInstance<ICommandBus>(commandBus);
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<ICommandHandlerRegistry>(commandProcessor);
            container.RegisterInstance<IMessageProcessor>("CommandProcessor", commandProcessor);
            container.RegisterInstance<IEventHandlerRegistry>(eventProcessor);
            container.RegisterInstance<IMessageProcessor>("EventProcessor", eventProcessor);

            // Event log database and handler
            this.RegisterMessageLogger(container, serializer, metadata, eventProcessor, config.MessageLogConnectionString);

            // Event Store
            this.RegisterEventStore(container, config.EventStoreConnectionString);

            // Bounded Context Registration
            if (domainContainer.DomainRegistrationList.Any())
                foreach (var registry in domainContainer.DomainRegistrationList)
                    registry(container, eventProcessor);

            // Handlers
            this.RegisterCommandHandlers(container);
            this.RegisterAditionalEventHandlers(container, eventProcessor);

            return container;
        }

        private void RegisterMessageLogger(UnityContainer container, ITextSerializer serializer, IMetadataProvider metadata, EventProcessor eventProcessor, string connectionString)
        {
            //Database.SetInitializer<MessageLogDbContext>(null);
            container.RegisterType<MessageLog>(new InjectionConstructor(connectionString, serializer, metadata));
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
    }
}
