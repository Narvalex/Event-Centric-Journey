using System;

namespace Journey.EventSourcing.ReadModeling
{
    public interface IReadModelGenerator<T> where T : ReadModelDbContext
    {
        /// <summary>
        /// Projects an event to tables with ORM
        /// </summary>
        void Project(IVersionedEvent e, Action<T> doProjection, Action doRebuild);

        void Project(IVersionedEvent e, Action<T> doProjectionOrRebuild);

        void Consume<Log>(IVersionedEvent e, Action doConsume)
            where Log : class, IProcessedEvent, new();
    }
}
