using Journey.Messaging.Processing;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace Journey.Worker
{
    public interface IDomainContainer
    {
        List<Action<IUnityContainer, IEventHandlerRegistry>> DomainRegistrationList { get; }
    }
}
