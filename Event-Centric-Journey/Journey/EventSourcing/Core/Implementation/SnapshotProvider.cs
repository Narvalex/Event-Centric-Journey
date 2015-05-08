using Journey.Serialization;
using Journey.Utils;
using Journey.Utils.SystemTime;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Journey.EventSourcing
{
    public class SnapshotProvider : ISnapshotProvider
    {
        private readonly MemoryCache cache;
        private readonly ISystemTime time;
        private readonly Func<EventStoreDbContext> contextFactory;
        private readonly ITextSerializer serializer;
        private Action<string, IMemento, DateTime?> persistSnapshot;
        private static readonly object lockObject = new object();

        public SnapshotProvider(string name, ISystemTime time, Func<EventStoreDbContext> contextFactory, ITextSerializer serializer)
        {
            this.cache = new MemoryCache(name);
            this.time = time;
            this.contextFactory = contextFactory;
            this.serializer = serializer;

            this.persistSnapshot = (partitionKey, memento, dateTime) =>
            {
                using (var context = this.contextFactory.Invoke())
                {
                    lock (lockObject)
                    {
                        try
                        {
                            var storedSnapshot = context
                                .Snapshsots
                                .Where(x => x.PartitionKey == partitionKey)
                                .FirstOrDefault();

                            if (storedSnapshot == null)
                                context.AddToUnityOfWork(this.Serialize(partitionKey, memento, dateTime));
                            else
                            {
                                storedSnapshot.Memento = this.Serialize(partitionKey, memento, dateTime).Memento;
                                storedSnapshot.LastUpdateTime = dateTime;
                                context.AddToUnityOfWork(storedSnapshot);
                            }

                            context.SaveChanges();
                        }
                        catch (Exception)
                        { }
                    }
                }
            };
        }

        public void CacheMementoIfApplicable<T>(T source, string sourceType) where T : IEventSourced
        {
            var key = this.GetPartitionKey(source.Id, sourceType);
            var memento = ((IMementoOriginator)source).SaveToMemento();
            var dateTime = this.time.Now;
            this.cache.Set(
                key,
                new Tuple<IMemento, DateTime?>(memento, dateTime),
                new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) });

            Task.Factory.StartNew(() => this.persistSnapshot(key, memento, dateTime));
        }

        public Tuple<IMemento, DateTime?> GetMementoFromCache(Guid id, string sourceType)
        {
            var key = this.GetPartitionKey(id, sourceType);
            var memento = (Tuple<IMemento, DateTime?>)this.cache.Get(key);

            if (memento == null)
            {
                using (var context = this.contextFactory.Invoke())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;

                    var snapshot = context
                        .Snapshsots
                        .Where(x => x.PartitionKey == key)
                        .FirstOrDefault();

                    if (snapshot == null) return null;

                    memento = new Tuple<IMemento, DateTime?>(this.Deserialize(snapshot), snapshot.LastUpdateTime);
                }
            }

            return memento;
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
                    new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) });
            }
        }

        private string GetPartitionKey(Guid id, string sourceType)
        {
            return sourceType + "_" + id.ToString();
        }

        private IMemento Deserialize(RollingSnapshot snapshot)
        {
            using (var reader = new StringReader(snapshot.Memento))
            {
                return (IMemento)this.serializer.Deserialize(reader);
            }
        }

        private RollingSnapshot Serialize(string partitionKey, IMemento memento, DateTime? lastUpdateTime)
        {
            RollingSnapshot serialized;
            using (var writer = new StringWriter())
            {
                this.serializer.Serialize(writer, memento);
                serialized = new RollingSnapshot
                {
                    PartitionKey = partitionKey,
                    Memento = writer.ToString(),
                    LastUpdateTime = lastUpdateTime
                };
            }
            return serialized;
        }
    }
}
