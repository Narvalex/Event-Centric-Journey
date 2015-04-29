using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using Journey.Worker.Config;
using System;
using System.Collections.Generic;

namespace Journey.Worker.Rebuilding
{
    public abstract class DomainReadModelRebuilderRegistry<T> : IDomainReadModelRebuilderRegistry<T> where T : ReadModelDbContext
    {
        private readonly List<Action<T, IEventHandlerRegistry, IWorkerRoleTracer>> registrationList;
        private readonly IReadModelRebuilderConfig config;
        private readonly Func<T> contextFactory;

        public DomainReadModelRebuilderRegistry()
        {
            this.config = DefaultWorkerRoleConfigProvider.Configuration;

            this.registrationList = this.RegisterComplexEventProcessors();
            this.contextFactory = this.RegisterContextFactory();
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

        protected abstract List<Action<T, IEventHandlerRegistry, IWorkerRoleTracer>> RegisterComplexEventProcessors();

        protected abstract Func<T> RegisterContextFactory();
    }
}
