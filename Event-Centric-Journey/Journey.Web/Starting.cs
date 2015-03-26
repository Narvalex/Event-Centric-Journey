using Journey.Database;
using Journey.Messaging.Processing;
using Journey.Worker;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;

namespace Journey.Web.App_Start
{
    partial class UnityConfig
    {
        static partial void Start()
        {
            DatabaseSetup.Initialize();
            
            var worker = new WorkerRole(new DomainContainer());

            WorkerRoleManager.CreateNew(worker).StartWorking();
        }
    }

    public class DomainContainer : IDomainContainer
    {
        public List<Action<IUnityContainer, IEventHandlerRegistry>> DomainRegistrationList
        {
            get { return new List<Action<IUnityContainer, IEventHandlerRegistry>>(); }
        }
    }
}