using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using Journey.Worker;
using Microsoft.Practices.Unity;
using SimpleInventario.Handlers;
using SimpleInventario.ReadModel;
using SimpleInventario.ReadModeling;
using SimpleInventario.Reporting;
using System;

namespace SimpleInventario.Worker
{
    public class SimpleInventarioWorkerRegistry : DomainWorkerRegistry
    {
        public SimpleInventarioWorkerRegistry()
        {
            this.boundedContextFactoryList.Add(this.RegistrarUnicoBoundedContext);
        }

        /// <summary>
        /// Un ejemplo de cómo registrar bounded contexts
        /// </summary>
        private void RegistrarUnicoBoundedContext(IUnityContainer container, IEventHandlerRegistry liveEventProcessor, IEventHandlerRegistry rebuildReadModelEventProcessor)
        {
            // Commanding
            container.RegisterType<ICommandHandler, InventarioHandler>("InventarioHandler");
            
            // Reporting
            liveEventProcessor.Register(container.Resolve<AnimalesDeTodosLosPeriodosHandler>());

            // ReadModeling
            Func<SimpleInventarioDbContext> contextFactory = () => new SimpleInventarioDbContext(this.WorkerRoleConfig.ReadModelConnectionString);
            container.RegisterType<IReadModelGeneratorEngine<SimpleInventarioDbContext>, ReadModelGeneratorEngine<SimpleInventarioDbContext>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    contextFactory,
                    container.Resolve<IWorkerRoleTracer>()));

            liveEventProcessor.Register(container.Resolve<SimpleInventarioReadModelGenerator>());
        }
    }
}
