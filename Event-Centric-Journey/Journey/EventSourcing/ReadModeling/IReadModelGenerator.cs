using System;

namespace Journey.EventSourcing.ReadModeling
{
    public interface IReadModelGenerator<T> where T : ReadModelDbContext
    {
        /// <summary>
        /// Proyecta y hace rebuild con dos lógicas distintas.
        /// </summary>
        void Project(IVersionedEvent e, Action<T> doLiveProjection, Action<T> doRebuild);

        /// <summary>
        /// Proyecta y hace rebuild con una misma lógica (append only)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="doLiveProjectionOrRebuild"></param>
        void Project(IVersionedEvent e, Action<T> doLiveProjectionOrRebuild);

        void Consume<Log>(IVersionedEvent e, Action doConsume)
            where Log : class, IProcessedEvent, new();
    }
}
