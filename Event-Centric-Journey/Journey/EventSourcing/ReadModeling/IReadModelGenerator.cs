using System;

namespace Journey.EventSourcing.ReadModeling
{
    public interface IReadModelGenerator<T> where T : ReadModelDbContext
    {
        /// <summary>
        /// Projects an event to tables with ORM
        /// </summary>
        void Project(IVersionedEvent e, Action<T> doProjection, bool isLiveProjection = true);

        void Consume<Log>(IVersionedEvent e, Action doConsume, bool isLiveConsuming = true)
            where Log : class, IProcessedEvent, new();
    }
}
