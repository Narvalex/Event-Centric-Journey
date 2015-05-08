
using System;
namespace Journey.EventSourcing
{
    /// <summary>
    /// Provides a rolling snapshot provider
    /// </summary>
    public interface ISnapshotProvider
    {
        void CacheMementoIfApplicable<T>(T source, string sourceType) where T : IEventSourced;

        Tuple<IMemento, DateTime?> GetMementoFromCache(Guid id, string sourceType);

        void MarkCacheAsStale(Guid id, string sourceType);
    }
}
