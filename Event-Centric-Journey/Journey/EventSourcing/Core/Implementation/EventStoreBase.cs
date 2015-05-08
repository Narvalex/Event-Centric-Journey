using Journey.Serialization;
using Journey.Utils.SystemTime;
using Journey.Worker;
using System;
using System.Collections.Generic;
using System.IO;

namespace Journey.EventSourcing
{
    public abstract class EventStoreBase<T> : IEventStore<T> where T : class, IEventSourced
    {
        // Could potentially use DataAnnotations to get a friendly/unique name in case of collisions between BCs.
        protected static readonly string _sourceType = typeof(T).Name;

        protected readonly IWorkerRoleTracer tracer;
        private readonly ITextSerializer serializer;
        protected readonly ISystemTime dateTime;
        protected readonly ISnapshotProvider snapshoter;
        protected readonly Func<Guid, IEnumerable<IVersionedEvent>, T> entityFactory;

        protected readonly Func<Guid, Tuple<IMemento, DateTime?>> getMementoFromCache;
        protected readonly Action<Guid> markCacheAsStale;
        protected readonly Func<Guid, IMemento, IEnumerable<IVersionedEvent>, T> originatorEntityFactory;
        protected readonly Action<T> cacheMementoIfApplicable;

        public EventStoreBase(IWorkerRoleTracer tracer, ITextSerializer serializer, ISystemTime dateTime, ISnapshotProvider snapshoter)
        {
            this.tracer = tracer;
            this.serializer = serializer;
            this.dateTime = dateTime;
            this.snapshoter = snapshoter;

            // TODO: could be replaced with a compiled lambda
            var constructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<IVersionedEvent>) });
            if (constructor == null)
                throw new InvalidCastException("Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IVersionedEvent>)");

            this.entityFactory = (id, events) => (T)constructor.Invoke(new object[] { id, events });

            if (typeof(IMementoOriginator).IsAssignableFrom(typeof(T)) && this.snapshoter != null)
            {
                // TODO: could be replaced with a compiled lambda to make it more performant
                var mementoConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IMemento), typeof(IEnumerable<IVersionedEvent>) });
                if (mementoConstructor == null)
                    throw new InvalidCastException(
                        "Type T must have a constructor with the following signature: .ctor(Guid, IMemento, IEnumerable<IVersionedEvent>)");
                this.originatorEntityFactory = (id, memento, events) => (T)mementoConstructor.Invoke(new object[] { id, memento, events });

                this.cacheMementoIfApplicable = (T originator) => this.snapshoter.CacheMementoIfApplicable(originator, _sourceType);

                this.getMementoFromCache = id => this.snapshoter.GetMementoFromCache(id, _sourceType);

                this.markCacheAsStale = id => this.snapshoter.MarkCacheAsStale(id, _sourceType);
            }
            else
            {
                // if no cache object or is not a cache originator, then no-op
                this.cacheMementoIfApplicable = o => { };
                this.getMementoFromCache = id => { return null; };
                this.markCacheAsStale = id => { };
            }
        }

        public T Get(Guid id)
        {
            var entity = this.Find(id);
            if (entity == null)
                throw new EntityNotFoundException(id, _sourceType);

            return entity;
        }

        public abstract void Save(T eventSourced, Guid correlationId, DateTime creationDate);

        public abstract T Find(Guid id);

        protected Event Serialize(IVersionedEvent e)
        {
            Event serialized;
            using (var writer = new StringWriter())
            {
                this.serializer.Serialize(writer, e);
                serialized = new Event
                {
                    SourceId = e.SourceId,
                    SourceType = _sourceType,
                    Version = e.Version,
                    Payload = writer.ToString(),
                    CorrelationId = e.CorrelationId,
                    EventType = e.GetType().Name,
                    CreationDate = e.CreationDate,
                    LastUpdateTime = dateTime.Now
                };
            }

            return serialized;
        }

        protected IVersionedEvent Deserialize(Event @event)
        {
            using (var reader = new StringReader(@event.Payload))
            {
                return (IVersionedEvent)this.serializer.Deserialize(reader);
            }
        }
    }
}
