using Journey.Messaging.Processing;
using Journey.Worker.Config;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace Journey.Worker
{
    public class DomainComponents : IDomainComponents
    {
        private readonly List<Action<IUnityContainer, IEventHandlerRegistry>> domainRegistrationList;
        private readonly IWorkerRoleConfig workerRoleConfig;

        public DomainComponents()
        {
            this.domainRegistrationList = new List<Action<IUnityContainer, IEventHandlerRegistry>>();
            this.workerRoleConfig = DefaultWorkerRoleConfigProvider.Configuration;
        }

        public List<Action<IUnityContainer, IEventHandlerRegistry>> DomainRegistrationList
        {
            get { return this.domainRegistrationList; }
        }


        public IWorkerRoleConfig WorkerRoleConfig
        {
            get { return this.workerRoleConfig; }
        }
    }
}
