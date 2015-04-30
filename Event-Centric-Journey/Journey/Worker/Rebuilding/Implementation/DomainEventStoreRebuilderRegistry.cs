using Journey.Messaging.Processing;
using Journey.Worker.Config;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace Journey.Worker.Rebuilding
{
    public abstract class DomainEventStoreRebuilderRegistry : IDomainEventStoreRebuilderRegistry
    {
        private readonly List<Action<IUnityContainer, IEventHandlerRegistry>> registrationList;
        private readonly IEventStoreRebuilderConfig config;

        public DomainEventStoreRebuilderRegistry()
        {
            this.config = DefaultWorkerRoleConfigProvider.Configuration;

            this.registrationList = RegisterComplexEventProcessors(new List<Action<IUnityContainer, IEventHandlerRegistry>>());
        }

        public List<Action<IUnityContainer, IEventHandlerRegistry>> RegistrationList
        {
            get { throw new NotImplementedException(); }
        }

        public IEventStoreRebuilderConfig Config
        {
            get { return this.config; }
        }

        protected abstract List<Action<IUnityContainer, IEventHandlerRegistry>> RegisterComplexEventProcessors(List<Action<IUnityContainer, IEventHandlerRegistry>> list);
    }
}
