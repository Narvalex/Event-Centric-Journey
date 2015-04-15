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
        private void RegistrarUnicoBoundedContext(IUnityContainer container, IEventHandlerRegistry eventProcessor)
        {
            // Commanding
            container.RegisterType<ICommandHandler, InventarioHandler>("InventarioHandler");
            
            // Reporting
            eventProcessor.Register(container.Resolve<AnimalesDeTodosLosPeriodosHandler>());

            // ReadModeling
            Func<SimpleInventarioDbContext> contextFactory = () => new SimpleInventarioDbContext(this.WorkerRoleConfig.ReadModelConnectionString);
            container.RegisterType<IReadModelGenerator<SimpleInventarioDbContext>, ReadModelGenerator<SimpleInventarioDbContext>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    contextFactory,
                    container.Resolve<IWorkerRoleTracer>()));

            eventProcessor.Register(container.Resolve<SimpleInventarioReadModelBuilder>());
        }
    }
}
