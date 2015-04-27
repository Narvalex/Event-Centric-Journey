using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using Journey.Worker.Config;
using System;
using System.Collections.Generic;

namespace Journey.Worker.Rebuilding
{
    public abstract class DomainReadModelRebuilderRegistry<T> : IDomainReadModelRebuilderRegistry<T> where T : ReadModelDbContext
    {
        protected readonly List<Action<T, IEventHandlerRegistry, IWorkerRoleTracer>> registrationList;
        private readonly IReadModelRebuilderConfig config;
        protected Func<T> contextFactory;

        public DomainReadModelRebuilderRegistry()
        {
            this.registrationList = new List<Action<T, IEventHandlerRegistry, IWorkerRoleTracer>>();
            this.config = DefaultWorkerRoleConfigProvider.Configuration;

            this.RegisterComplexEventProcessors();
            this.RegisterContextFactory();
        }

        public List<Action<T, IEventHandlerRegistry, IWorkerRoleTracer>> RegistrationList
        {
            get { return this.registrationList; }
        }

        public Func<T> ContextFactory
        {
            get { return this.contextFactory; }
        }

        public IReadModelRebuilderConfig Config
        {
            get { return this.config; }
        }

        protected abstract void RegisterComplexEventProcessors();

        protected abstract void RegisterContextFactory();
    }
}
