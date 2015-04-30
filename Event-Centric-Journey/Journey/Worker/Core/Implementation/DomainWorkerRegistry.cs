using Journey.Messaging.Processing;
using Journey.Worker.Config;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace Journey.Worker
{
    public abstract class DomainWorkerRegistry : IDomainWorkerRegistry
    {
        private readonly List<Action<IUnityContainer, IEventHandlerRegistry>> registrationList;
        protected readonly IWorkerRoleConfig config;

        public DomainWorkerRegistry()
        {
            this.config = DefaultWorkerRoleConfigProvider.Configuration;

            this.registrationList = this.RegisterComplexEventProcessors(new List<Action<IUnityContainer, IEventHandlerRegistry>>());
        }

        public List<Action<IUnityContainer, IEventHandlerRegistry>> RegistrationList
        {
            get { return this.registrationList; }
        }

        public IWorkerRoleConfig Config
        {
            get { return this.config; }
        }

        protected abstract List<Action<IUnityContainer, IEventHandlerRegistry>> RegisterComplexEventProcessors(List<Action<IUnityContainer, IEventHandlerRegistry>> list);
    }
}
