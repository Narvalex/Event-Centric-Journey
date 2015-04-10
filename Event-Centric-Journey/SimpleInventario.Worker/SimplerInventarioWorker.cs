using Journey.Messaging.Processing;
using Journey.Worker;
using Microsoft.Practices.Unity;
using SimpleInventario.Handlers;

namespace SimpleInventario.Worker
{
    public class SimplerInventarioWorker : DomainWorker
    {
        public SimplerInventarioWorker()
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
