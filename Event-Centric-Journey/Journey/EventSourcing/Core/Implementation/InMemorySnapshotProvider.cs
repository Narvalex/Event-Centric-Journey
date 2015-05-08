using Journey.Utils.SystemTime;
using System;
using System.Runtime.Caching;

namespace Journey.EventSourcing
{
    public class InMemorySnapshotProvider : ISnapshotProvider
    {
        private readonly MemoryCache cache;
        private readonly ISystemTime time;


        public InMemorySnapshotProvider(string name, ISystemTime time)
        {
            this.cache = new MemoryCache(name);
            this.time = time;
        }

        public void CacheMementoIfApplicable<T>(T source, string sourceType) where T : IEventSourced
        {
            var key = this.GetPartitionKey(source.Id, sourceType);
            var memento = ((IMementoOriginator)source).SaveToMemento();
            this.cache.Set(
                key,
                new Tuple<IMemento, DateTime?>(memento, this.time.Now),
                //new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) });
                new CacheItemPolicy { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration });
        }

        public Tuple<IMemento, DateTime?> GetMementoFromCache(Guid id, string sourceType)
        {
            return (Tuple<IMemento, DateTime?>)this.cache.Get(this.GetPartitionKey(id, sourceType));
        }

        public void MarkCacheAsStale(Guid id, string sourceType)
        {
            var key = this.GetPartitionKey(id, sourceType);
            var item = (Tuple<IMemento, DateTime?>)this.cache.Get(key);
            if (item != null && item.Item2.HasValue)
            {
                item = new Tuple<IMemento, DateTime?>(item.Item1, null);
                this.cache.Set(
                    key,
                    item,
                    //new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) });
                    new CacheItemPolicy { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration });
            }
        }

        private string GetPartitionKey(Guid id, string sourceType)
        {
            return sourceType + "_" + id.ToString();
        }
    }
}
