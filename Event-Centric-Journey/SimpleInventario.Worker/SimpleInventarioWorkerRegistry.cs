using Journey.Messaging.Processing;
using Journey.Worker;
using Microsoft.Practices.Unity;
using SimpleInventario.Handlers;

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
            container.RegisterType<ICommandHandler, InventarioHandler>("InventarioHandler");
        }
    }
}
