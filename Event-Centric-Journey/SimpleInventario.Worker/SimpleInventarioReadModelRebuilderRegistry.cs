using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using Journey.Worker;
using Journey.Worker.Rebuilding;
using SimpleInventario.ReadModel;
using SimpleInventario.ReadModeling;

namespace SimpleInventario.Worker
{
    public class SimpleInventarioReadModelRebuilderRegistry : DomainReadModelRebuilderRegistry<SimpleInventarioDbContext>
    {
        protected override void RegisterComplexEventProcessors()
        {
            this.registrationList.Add(this.RegistrarUnicoBoundedContext);
        }

        protected override void RegisterContextFactory()
        {
            this.contextFactory = () => new SimpleInventarioDbContext(this.Config.EventStoreConnectionString);
        }

        private void RegistrarUnicoBoundedContext(SimpleInventarioDbContext context, IEventHandlerRegistry rebuildEventProcessor, IWorkerRoleTracer tracer)
        {
            var readModelGeneratorEngine = new ReadModelGeneratorEngine<SimpleInventarioDbContext>(context, tracer);
            var readModelGenerator = new SimpleInventarioReadModelGenerator(readModelGeneratorEngine);

            rebuildEventProcessor.Register(readModelGenerator);
        }
    }
}
