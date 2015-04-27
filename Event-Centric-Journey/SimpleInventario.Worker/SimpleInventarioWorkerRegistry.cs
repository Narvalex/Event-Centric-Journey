using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using Journey.Worker;
using Microsoft.Practices.Unity;
using SimpleInventario.Handlers;
using SimpleInventario.ReadModel;
using SimpleInventario.ReadModeling;
using SimpleInventario.Reporting;
using System;
using System.Collections.Generic;

namespace SimpleInventario.DomainRegistry
{
    public class SimpleInventarioWorkerRegistry : DomainWorkerRegistry
    {
        protected override List<Action<IUnityContainer, IEventHandlerRegistry>> RegisterComplexEventProcessors()
        {
            var registrationList = new List<Action<IUnityContainer, IEventHandlerRegistry>>();
            registrationList.Add(this.RegistrarUnicoBoundedContext);
            return registrationList;
        }

        /// <summary>
        /// Un ejemplo de cómo registrar bounded contexts
        /// </summary>
        private void RegistrarUnicoBoundedContext(IUnityContainer container, IEventHandlerRegistry liveEventProcessor)
        {
            // Commanding
            container.RegisterType<ICommandHandler, InventarioHandler>("InventarioHandler");
            
            // Reporting
            liveEventProcessor.Register(container.Resolve<AnimalesDeTodosLosPeriodosHandler>());

            // ReadModeling
            Func<SimpleInventarioDbContext> contextFactory = () => new SimpleInventarioDbContext(this.Config.ReadModelConnectionString);
            container.RegisterType<IReadModelGeneratorEngine<SimpleInventarioDbContext>, ReadModelGeneratorEngine<SimpleInventarioDbContext>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    contextFactory,
                    container.Resolve<IWorkerRoleTracer>()));

            liveEventProcessor.Register(container.Resolve<SimpleInventarioReadModelGenerator>());
        }


    }
}
