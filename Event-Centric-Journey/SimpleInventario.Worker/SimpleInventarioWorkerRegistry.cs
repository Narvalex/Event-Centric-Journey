using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using Journey.Worker;
using Journey.Worker.Config;
using Microsoft.Practices.Unity;
using SimpleInventario.ReadModel;
using SimpleInventario.ReadModeling;
using System;
using System.Collections.Generic;

namespace SimpleInventario.DomainRegistry
{
    public class SimpleInventarioWorkerRegistry : DomainWorkerRegistry
    {
        protected override List<Action<IUnityContainer, IEventHandlerRegistry>> RegisterComplexEventProcessors(List<Action<IUnityContainer, IEventHandlerRegistry>> list)
        {
            list.Add(SharedRegistry.SimpleInventarioRegistrarUnicoBoundedContext);
            list.Add(this.RegistrarUnicoBoundedContext);

            return list;
        }

        /// <summary>
        /// Un ejemplo de cómo registrar bounded contexts
        /// </summary>
        private void RegistrarUnicoBoundedContext(IUnityContainer container, IEventHandlerRegistry liveEventProcessor)
        {
            // ReadModeling
            Func<SimpleInventarioDbContext> contextFactory = () => new SimpleInventarioDbContext(this.Config.ReadModelConnectionString);
            container.RegisterType<IReadModelGeneratorEngine<SimpleInventarioDbContext>, ReadModelGeneratorEngine<SimpleInventarioDbContext>>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    contextFactory,
                    container.Resolve<IWorkerRoleTracer>()));

            liveEventProcessor.Register(container.Resolve<SimpleInventarioReadModelGenerator>());
        }

        protected override Journey.Worker.Config.IWorkerRoleConfig RegisterWorkerConfig()
        {
            return DefaultWorkerRoleConfigProvider.Configuration;
        }
    }
}
