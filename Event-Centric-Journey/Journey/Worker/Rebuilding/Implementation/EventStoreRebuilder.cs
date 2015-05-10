using Journey.Database;
using Journey.EventSourcing;
using Journey.EventSourcing.EventStoreRebuilding;
using Journey.Messaging;
using Journey.Messaging.Logging;
using Journey.Messaging.Logging.Metadata;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils.SystemTime;
using Journey.Worker.Config;
using Microsoft.Practices.Unity;
using System.Data.Entity;

namespace Journey.Worker.Rebuilding
{
    public class EventStoreRebuilder : IEventStoreRebuilder
    {
        private readonly IUnityContainer container;
        private readonly IWorkerRoleTracer tracer;
        private readonly IDomainEventStoreRebuilderRegistry domainRegistry;

        public EventStoreRebuilder(IDomainEventStoreRebuilderRegistry domainRegistry, IWorkerRoleTracer tracer)
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
            this.domainRegistry = domainRegistry;
            this.tracer = tracer;
            this.container = this.CreateContainer();
        }

        public void Rebuild()
        {
            container.Resolve<IEventStoreRebuilderEngine>()
                .Rebuild();
        }

        private IUnityContainer CreateContainer()
        {
            var container = new UnityContainer();

            var config = this.domainRegistry.Config;
            container.RegisterInstance<IEventStoreRebuilderConfig>(config);
            container.RegisterInstance<ISystemTime>(config.SystemTime);
            var commandProcessor = new InMemoryCommandProcessor(this.tracer);
            var eventProcessor = new SynchronousEventDispatcher(this.tracer);

            var bus = new InMemoryBus();

            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IWorkerRoleTracer>(this.tracer);

            var snapshoter = new InMemorySnapshotProvider("Snapshotter", config.SystemTime);
            container.RegisterInstance<ISnapshotProvider>(snapshoter);


            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());

            container.RegisterType<EventStoreDbContext>(new ContainerControlledLifetimeManager(), new InjectionConstructor(config.EventStoreConnectionString));
            container.RegisterType<MessageLogDbContext>(new ContainerControlledLifetimeManager(), new InjectionConstructor(config.SourceMessageLogConnectionString));
            container.RegisterType(typeof(IEventStore<>), typeof(InMemoryEventStore<>), new ContainerControlledLifetimeManager());

            container.RegisterInstance<IEventDispatcher>(eventProcessor);
            container.RegisterInstance<ICommandProcessor>(commandProcessor);
            container.RegisterInstance<ICommandHandlerRegistry>(commandProcessor);

            container.RegisterInstance<IInMemoryBus>(bus);

            container.RegisterType(typeof(IEventStoreRebuilderEngine), typeof(EventStoreRebuilderEngine), new ContainerControlledLifetimeManager());

            foreach (var registrationAction in this.domainRegistry.RegistrationList)
                registrationAction(container, eventProcessor);

            this.RegisterCommandHandlers(container);

            return container;
        }

        private void RegisterCommandHandlers(IUnityContainer container)
        {
            var commandHandlerRegistry = container.Resolve<ICommandHandlerRegistry>();

            foreach (var commandHandler in container.ResolveAll<ICommandHandler>())
                commandHandlerRegistry.Register(commandHandler);
        }
    }
}
