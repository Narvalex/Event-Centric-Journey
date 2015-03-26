using Journey.Messaging.Processing;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace Journey.Worker
{
    public class DomainContainer : IDomainContainer
    {
        private readonly List<Action<IUnityContainer, IEventHandlerRegistry>> domainRegistrationList;

        public DomainContainer()
        {
            this.domainRegistrationList = new List<Action<IUnityContainer, IEventHandlerRegistry>>();
        }

        public List<Action<IUnityContainer, IEventHandlerRegistry>> DomainRegistrationList
        {
            get { return this.domainRegistrationList; }
        }
    }
}
