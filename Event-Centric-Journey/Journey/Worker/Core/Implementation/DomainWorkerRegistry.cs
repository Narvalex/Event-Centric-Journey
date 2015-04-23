using Journey.Messaging.Processing;
using Journey.Worker.Config;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace Journey.Worker
{
    public abstract class DomainWorkerRegistry : IDomainWorkerRegistry
    {
        protected readonly List<Action<IUnityContainer, IEventHandlerRegistry, IEventHandlerRegistry>> boundedContextFactoryList;
        private readonly IWorkerRoleConfig workerRoleConfig;

        public DomainWorkerRegistry()
        {
            this.boundedContextFactoryList = new List<Action<IUnityContainer, IEventHandlerRegistry, IEventHandlerRegistry>>();
            this.workerRoleConfig = DefaultWorkerRoleConfigProvider.Configuration;
        }

        public List<Action<IUnityContainer, IEventHandlerRegistry, IEventHandlerRegistry>> DomainRegistrationList
        {
            get { return this.boundedContextFactoryList; }
        }


        public IWorkerRoleConfig WorkerRoleConfig
        {
            get { return this.workerRoleConfig; }
        }
    }
}
