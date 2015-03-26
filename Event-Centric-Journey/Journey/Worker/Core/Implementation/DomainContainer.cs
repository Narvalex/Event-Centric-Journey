using Journey.Messaging.Processing;
using Journey.Worker.Config;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace Journey.Worker
{
    public class DomainContainer : IDomainContainer
    {
        private readonly List<Action<IUnityContainer, IEventHandlerRegistry>> domainRegistrationList;
        private readonly IWorkerRoleConfig workerRoleConfig;

        public DomainContainer()
        {
            this.domainRegistrationList = new List<Action<IUnityContainer, IEventHandlerRegistry>>();
            this.workerRoleConfig = DefaultConfigProvider.Configuration;
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
