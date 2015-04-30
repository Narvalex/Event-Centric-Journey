using Journey.Messaging.Processing;
using Journey.Worker.Config;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace Journey.Worker.Rebuilding
{
    interface IDomainEventStoreRebuilderRegistry
    {
        /// <summary>
        /// El contenedor, los live event processors y los read model rebuilders processors 
        /// </summary>
        List<Action<IUnityContainer, IEventHandlerRegistry>> RegistrationList { get; }

        IEventStoreRebuilderConfig Config { get; }
    }
}
