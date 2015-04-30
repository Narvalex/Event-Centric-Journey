using Journey.Messaging.Processing;
using Microsoft.Practices.Unity;
using SimpleInventario.Handlers;
using SimpleInventario.Reporting;

namespace SimpleInventario.DomainRegistry
{
    public static class SharedRegistry
    {
        public static void SimpleInventarioRegistrarUnicoBoundedContext
            (IUnityContainer container, IEventHandlerRegistry eventProcessor)
        {
            // Commanding
            container.RegisterType<ICommandHandler, InventarioHandler>("InventarioHandler");

            // Reporting
            eventProcessor.Register(container.Resolve<AnimalesDeTodosLosPeriodosHandler>());
        }
    }
}
