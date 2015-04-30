using Journey.Messaging.Processing;
using Journey.Worker.Rebuilding;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace SimpleInventario.DomainRegistry
{
    public class SimpleInventarioEventStoreRebuilderRegistry : DomainEventStoreRebuilderRegistry
    {
        protected override List<Action<IUnityContainer, IEventHandlerRegistry>> RegisterComplexEventProcessors(List<Action<IUnityContainer, IEventHandlerRegistry>> list)
        {
            list.Add(SharedRegistry.SimpleInventarioRegistrarUnicoBoundedContext);
            return list;
        }
    }
}
