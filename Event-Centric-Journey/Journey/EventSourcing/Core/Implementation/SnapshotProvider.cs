using Journey.Serialization;
using Journey.Utils;
using Journey.Utils.SystemTime;
using Journey.Worker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;

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
        private readonly ITracer tracer;

        public SnapshotProvider(string name, ISystemTime time, Func<EventStoreDbContext> contextFactory, ITextSerializer serializer, ITracer tracer)
        {
            this.cache = new MemoryCache(name);
            this.time = time;
            this.contextFactory = contextFactory;
            this.serializer = serializer;
            this.tracer = tracer;

            this.persistSnapshot = (partitionKey, memento, dateTime) =>
            {
                var attempts = 0;
                var sourceName = memento.GetType().FullName;
                while (true)
                {
                    try
                    {
                        using (var context = this.contextFactory.Invoke())
                        {

                            var storedSnapshot = context
                                .Snapshsots
                                .Where(x => x.PartitionKey == partitionKey)
                                .FirstOrDefault();

                            if (storedSnapshot == null)
                            {
                                context.AddToUnityOfWork(this.Serialize(partitionKey, memento, dateTime));
                                context.SaveChanges();
                                this.tracer.TraceAsync(string.Format("Saved new snapshot! Source: {0}. Version: {1}", sourceName, memento.Version));
                                break;
                            }
                            else
                            {
                                lock (lockObject)
                                {
                                    storedSnapshot.Memento = this.Serialize(partitionKey, memento, dateTime).Memento;
                                    storedSnapshot.LastUpdateTime = dateTime;
                                    context.AddToUnityOfWork(storedSnapshot);
                                    context.SaveChanges();
                                    this.tracer.TraceAsync(string.Format("An snapshot has been uptaded! Source: {0}. Version: {1}", sourceName, memento.Version));
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ++attempts;
                        if (attempts >= 3)
                            break;

                        this.tracer.Notify(new List<string>
                        {
                            new string('-', 80),
                            string.Format(
                            "Persisting snapshot attempt number {0}. An exception happened while persisting snapshot {1} version {2}", attempts, sourceName, memento.Version),
                            new string('-', 80)
                        }
                        .ToArray());

                        if (attempts > 1)
                            Thread.Sleep(attempts * 1000);
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

            ThreadPool.QueueUserWorkItem(x => this.persistSnapshot(key, memento, dateTime));
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
