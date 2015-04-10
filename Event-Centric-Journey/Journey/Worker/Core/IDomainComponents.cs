using Journey.Messaging.Processing;
using Journey.Worker.Config;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace Journey.Worker
{
    public interface IDomainComponents
    {
        List<Action<IUnityContainer, IEventHandlerRegistry>> DomainRegistrationList { get; }

        IWorkerRoleConfig WorkerRoleConfig { get; }
    }
}
