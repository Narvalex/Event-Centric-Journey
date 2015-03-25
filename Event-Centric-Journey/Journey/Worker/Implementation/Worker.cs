using Journey.Database;
using Journey.EventSourcing;
using Journey.Messaging;
using Journey.Messaging.Logging;
using Journey.Messaging.Logging.Metadata;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Journey.Worker
{
    public class Worker : IWorker
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly IUnityContainer container;
        private readonly List<IMessageProcessor> processors;

        private static object lockObject = new object();
        public static IWorkerRoleTracer Tracer;
        public static readonly Queue<Notification> Notifications = new Queue<Notification>(50);
        public static int NotificationCountLimit = 50;
        public static volatile int NotificationCount = default(int);

        public Worker(IDomainContainer domainContainer)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.CreateWebTracer();
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

        private void CreateWebTracer()
        {
            Tracer = new WebWorkerTracer((n) =>
            {
                lock (lockObject)
                {
                    if (Notifications.Count >= NotificationCountLimit)
                        Notifications.Dequeue();

                    Notifications.Enqueue(new Notification
                    {
                        id = ++NotificationCount,
                        message = string.Format("{0} - {1}", DateTime.Now.ToString(), n)
                    });
                }
            });
        }

        private IUnityContainer CreateContainer(IDomainContainer domainContainer)
        {
            var container = new UnityContainer();

            //TODO: in config file
            var messagingSettings = new MessagingSettings(1, TimeSpan.FromMinutes(100));
            var connectionProvider = new ConnectionStringProvider("Data Source=WS11;Initial Catalog=SIRDAT_P9;User ID=so;Password=1joca395;Persist Security Info=True;packet size=4096");

            // Infrastructure
            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());
            container.RegisterInstance<IWorkerRoleTracer>(Tracer);
            container.RegisterInstance<IMessagingSettings>(messagingSettings);
            container.RegisterInstance<IConnectionStringProvider>(connectionProvider);


            var serializer = container.Resolve<ITextSerializer>();
            var metadata = container.Resolve<IMetadataProvider>();
            var tracer = container.Resolve<IWorkerRoleTracer>();

            var commandBus = new CommandBus(new MessageSender(System.Data.Entity.Database.DefaultConnectionFactory, "Bus", "Bus.Commands"), serializer);
            var eventBus = new EventBus(new MessageSender(System.Data.Entity.Database.DefaultConnectionFactory, "Bus", "Bus.Events"), serializer);

            var commandProcessor = new CommandProcessor(new MessageReceiver(System.Data.Entity.Database.DefaultConnectionFactory, "Bus", "Bus.Commands", messagingSettings.BusPollDelay, messagingSettings.NumberOfThreads), serializer, tracer, new BusTransientFaultDetector(connectionProvider));
            var eventProcessor = new EventProcessor(new MessageReceiver(System.Data.Entity.Database.DefaultConnectionFactory, "Bus", "Bus.Events", messagingSettings.BusPollDelay, messagingSettings.NumberOfThreads), serializer, tracer);

            var inMemorySnapshotCache = new InMemorySnapshotCache("EventStoreCache");

            container.RegisterInstance<ISnapshotCache>(inMemorySnapshotCache);
            container.RegisterInstance<ICommandBus>(commandBus);
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<ICommandHandlerRegistry>(commandProcessor);
            container.RegisterInstance<IMessageProcessor>("CommandProcessor", commandProcessor);
            container.RegisterInstance<IEventHandlerRegistry>(eventProcessor);
            container.RegisterInstance<IMessageProcessor>("EventProcessor", eventProcessor);

            // Event log database and handler
            this.RegisterMessageLogger(container, serializer, metadata, eventProcessor);

            // Event Store
            this.RegisterEventStore(container);

            // Bounded Context Registration
            if (domainContainer.DomainRegistrationList.Any())
                foreach (var registry in domainContainer.DomainRegistrationList)
                    registry(container, eventProcessor);

            // Handlers
            this.RegisterCommandHandlers(container);
            this.RegisterAditionalEventHandlers(container, eventProcessor);

            return container;
        }

        private void RegisterMessageLogger(UnityContainer container, ITextSerializer serializer, IMetadataProvider metadata, EventProcessor eventProcessor)
        {
            //Database.SetInitializer<MessageLogDbContext>(null);
            container.RegisterType<MessageLog>(new InjectionConstructor("MessageLog", serializer, metadata));
            container.RegisterType<IEventHandler, MessageLogHandler>("MessageLogHandler");
            container.RegisterType<ICommandHandler, MessageLogHandler>("MessageLogHandler");
            eventProcessor.Register(container.Resolve<MessageLogHandler>());
        }

        private void RegisterEventStore(IUnityContainer container)
        {
            //Database.SetInitializer<EventStoreDbContext>(null);
            container.RegisterType<EventStoreDbContext>(new TransientLifetimeManager(), new InjectionConstructor("EventStore"));
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
        public class Notification
        {
            public int id { get; set; }
            public string message { get; set; }
        }


        public void Dispose()
        {
            this.container.Dispose();
            this.cancellationTokenSource.Dispose();
        }
    }
}
