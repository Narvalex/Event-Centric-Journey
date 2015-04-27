using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using Journey.Worker.Config;
using System;
using System.Collections.Generic;

namespace Journey.Worker.Rebuilding
{
    public interface IDomainReadModelRebuilderRegistry<T> where T : ReadModelDbContext
    {
        List<Action<T, IEventHandlerRegistry, IWorkerRoleTracer>> RegistrationList { get; }

        Func<T> ContextFactory { get; }

        IReadModelRebuilderConfig Config { get; }
    }
}
