using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using Journey.Worker;
using Journey.Worker.Config;
using Journey.Worker.Rebuilding;
using SimpleInventario.ReadModel;
using SimpleInventario.ReadModeling;
using System;
using System.Collections.Generic;

namespace SimpleInventario.Worker
{
    public class SimpleInventarioReadModelRebuilderRegistry : DomainReadModelRebuilderRegistry<SimpleInventarioDbContext>
    {
        protected override List<Action<SimpleInventarioDbContext, IEventHandlerRegistry, IWorkerRoleTracer>> RegisterComplexEventProcessors()
        {
            return new List<Action<SimpleInventarioDbContext, IEventHandlerRegistry, IWorkerRoleTracer>>
            {
                this.RegistrarUnicoBoundedContext
            };
        }

        protected override Func<SimpleInventarioDbContext> RegisterContextFactory()
        {
            return () => new SimpleInventarioDbContext(this.Config.ReadModelConnectionString);
        }

        private void RegistrarUnicoBoundedContext(SimpleInventarioDbContext context, IEventHandlerRegistry rebuildEventProcessor, IWorkerRoleTracer tracer)
        {
            var readModelGeneratorEngine = new ReadModelGeneratorEngine<SimpleInventarioDbContext>(context, tracer);
            var readModelGenerator = new SimpleInventarioReadModelGenerator(readModelGeneratorEngine);

            rebuildEventProcessor.Register(readModelGenerator);
        }

        protected override IReadModelRebuilderConfig RegisterConfig()
        {
            return DefaultWorkerRoleConfigProvider.Configuration;
        }
    }
}
