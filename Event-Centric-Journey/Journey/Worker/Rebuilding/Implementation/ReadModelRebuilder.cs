using Journey.Database;
using Journey.EventSourcing;
using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using Journey.Serialization;
using System;
using System.Data.Entity;

namespace Journey.Worker.Rebuilding
{
    public class ReadModelRebuilder<T> : IReadModelRebuilder<T>
        where T : ReadModelDbContext
    {
        private readonly IWorkerRoleTracer tracer;
        private readonly IDomainReadModelRebuilderRegistry<T> domainRegistry;

        public ReadModelRebuilder(IDomainReadModelRebuilderRegistry<T> domainRegistry, IWorkerRoleTracer tracer)
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
            this.tracer = tracer;
            this.domainRegistry = domainRegistry;
        }

        public void Rebuild()
        {
            var complexEventProcessor = new SynchronousEventDispatcher(this.tracer);

            var serializer = new JsonTextSerializer();

            using (var context = domainRegistry.ContextFactory.Invoke())
            {
                foreach (var registrationAction in domainRegistry.RegistrationList)
                    registrationAction(context, complexEventProcessor, this.tracer);

                Func<EventStoreDbContext> eventStoreContextFactory = () => new EventStoreDbContext(domainRegistry.Config.EventStoreConnectionString);

                var engine = new ReadModelRebuilderEngine<T>(eventStoreContextFactory, serializer, complexEventProcessor, context);

                engine.Rebuild();
            }
        }
    }
}
