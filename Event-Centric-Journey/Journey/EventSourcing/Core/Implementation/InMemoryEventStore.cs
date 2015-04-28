using Journey.Messaging;
using Journey.Serialization;
using Journey.Utils;
using Journey.Worker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;

namespace Journey.EventSourcing
{
    /// <summary>
    /// This is an extremely basic implementation of the event store (straw man), that is used only for running the application
    /// without the dependency to the Windows Azure Service Bus.
    /// It does check for event versions before committing, but is not transactional with the event bus nor resilient to connectivity errors or crashes.
    /// It does do snapshots for entities that implements <see cref="IMementoOriginator"/>.
    /// </summary>
    /// <typeparam name="T">The entity type to persist.</typeparam>
    public class InMemoryEventStore<T> : IEventStore<T> where T : class, IEventSourced
    {
        private readonly IWorkerRoleTracer tracer;

        // Could potentially use DataAnnotations to get a friendly/unique name in case of collisions between BCs.
        private static readonly string _sourceType = typeof(T).Name;
        private readonly IInMemoryEventBus eventBus;
        private readonly IInMemoryCommandBus commandBus;
        private readonly ITextSerializer serializer;
        private readonly EventStoreDbContext context;
        private readonly Func<Guid, IEnumerable<IVersionedEvent>, T> entityFactory;
        private readonly Action<T> cacheMementoIfApplicable;
        private readonly IInMemoryRollingSnapshotProvider cache;
        private readonly Func<Guid, Tuple<IMemento, DateTime?>> getMementoFromCache;
        private readonly Action<Guid> markCacheAsStale;
        private readonly Func<Guid, IMemento, IEnumerable<IVersionedEvent>, T> originatorEntityFactory;

        public InMemoryEventStore(IInMemoryEventBus eventBus, IInMemoryCommandBus commandBus, ITextSerializer serializer, EventStoreDbContext context, IInMemoryRollingSnapshotProvider cache, IWorkerRoleTracer tracer)
        {
            this.eventBus = eventBus;
            this.commandBus = commandBus;
            this.serializer = serializer;
            this.context = context;
            this.cache = cache;

            // TODO: could be replaced with a compiled lambda
            var constructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IEnumerable<IVersionedEvent>) });
            if (constructor == null)
            {
                throw new InvalidCastException("Type T must have a constructor with the following signature: .ctor(Guid, IEnumerable<IVersionedEvent>)");
            }
            this.entityFactory = (id, events) => (T)constructor.Invoke(new object[] { id, events });

            if (typeof(IMementoOriginator).IsAssignableFrom(typeof(T)) && this.cache != null)
            {
                // TODO: could be replaced with a compiled lambda to make it more performant
                var mementoConstructor = typeof(T).GetConstructor(new[] { typeof(Guid), typeof(IMemento), typeof(IEnumerable<IVersionedEvent>) });
                if (mementoConstructor == null)
                    throw new InvalidCastException(
                        "Type T must have a constructor with the following signature: .ctor(Guid, IMemento, IEnumerable<IVersionedEvent>)");
                this.originatorEntityFactory = (id, memento, events) => (T)mementoConstructor.Invoke(new object[] { id, memento, events });
                this.cacheMementoIfApplicable = (T originator) =>
                {
                    var key = this.GetPartitionKey(originator.Id);
                    var memento = ((IMementoOriginator)originator).SaveToMemento();
                    this.cache.Set(
                        key,
                        new Tuple<IMemento, DateTime?>(memento, DateTime.Now),
                        //new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddYears(1) });
                        new CacheItemPolicy { AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration });
                };
                this.getMementoFromCache = id => (Tuple<IMemento, DateTime?>)this.cache.Get(this.GetPartitionKey(id));
                this.markCacheAsStale = id =>
                {
                    var key = this.GetPartitionKey(id);
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
                };
            }
            else
            {
                // if no cache object or is not a cache originator, then no-op
                this.cacheMementoIfApplicable = o => { };
                this.getMementoFromCache = id => { return null; };
                this.markCacheAsStale = id => { };
            }

            this.tracer = tracer;
        }

        public T Find(Guid id)
        {
            var cachedMemento = this.getMementoFromCache(id);
            if (cachedMemento != null && cachedMemento.Item1 != null)
            {
                // NOTE: if we had a guarantee that this is running in a single process, there is
                // no need to check if there are new events after the cached version.
                IEnumerable<IVersionedEvent> deserialized;
                if (!cachedMemento.Item2.HasValue || cachedMemento.Item2.Value < DateTime.Now.AddSeconds(-1))
                {

                    deserialized = this.context.Set<Event>()
                        .Local
                        .Where(x => x.AggregateId == id && x.AggregateType == _sourceType && x.Version > cachedMemento.Item1.Version)
                        .OrderBy(x => x.Version)
                        .AsEnumerable()
                        .Select(this.Deserialize)
                        .AsCachedAnyEnumerable();

                    if (deserialized.Any())
                        return entityFactory.Invoke(id, deserialized);

                }
                else
                {
                    // if the cache entry was updated in the last seconds, then there is a high possibility that it is not stale
                    // (because we typically have a single writer for high contention aggregates). This is why we optimistically avoid
                    // getting the new events from the EventStore since the last memento was created. In the low probable case
                    // where we get an exception on save, then we mark the cache item as stale so when the command gets
                    // reprocessed, this time we get the new events from the EventStore.
                    deserialized = Enumerable.Empty<IVersionedEvent>();
                }

                return this.originatorEntityFactory.Invoke(id, cachedMemento.Item1, deserialized);
            }
            else
            {

                var deserialized = this.context.Set<Event>()
                    .Local
                    .Where(x => x.AggregateId == id && x.AggregateType == _sourceType)
                    .OrderBy(x => x.Version)
                    .AsEnumerable()
                    .Select(this.Deserialize)
                    .AsCachedAnyEnumerable();

                if (deserialized.Any())
                {
                    return entityFactory.Invoke(id, deserialized);
                }

                return null;

            }
        }

        public T Get(Guid id)
        {
            var entity = this.Find(id);
            if (entity == null)
                throw new EntityNotFoundException(id, _sourceType);

            return entity;
        }

        public void Save(T eventSourced, Guid correlationId)
        {
            var events = eventSourced.Events.ToArray();

            if (events.Count() == 0)
            {
                var noEventsMessage = string.Format("Aggregate {0} with Id {1} HAS NO EVENTS to be saved.", _sourceType, eventSourced.Id.ToString());
                this.tracer.Notify(noEventsMessage);
                return;
            }

            ICommand[] commands = null;
            if (typeof(ISaga).IsAssignableFrom(typeof(T)))
                commands = (eventSourced as ISaga).Commands.ToArray();

            try
            {
                var eventsSet = this.context.Set<Event>();

                foreach (var e in events)
                {
                    // le pasamos el command id para que se serialice
                    e.CorrelationId = correlationId;
                    eventsSet.Add(this.Serialize(e));
                }

                var correlationIdString = correlationId.ToString();
                this.eventBus.Publish(events.Select(e => new Envelope<IEvent>(e) { CorrelationId = correlationIdString }));

                if (commands != null && commands.Count() > 0)
                    this.commandBus.Send(commands.Select(c => new Envelope<ICommand>(c) { CorrelationId = correlationIdString }));
            }
            catch (Exception)
            {

                this.markCacheAsStale(eventSourced.Id);
                throw;
            }

            this.cacheMementoIfApplicable.Invoke(eventSourced);
        }

        private Event Serialize(IVersionedEvent e)
        {
            Event serialized;
            using (var writer = new StringWriter())
            {
                this.serializer.Serialize(writer, e);
                serialized = new Event
                {
                    AggregateId = e.SourceId,
                    AggregateType = _sourceType,
                    Version = e.Version,
                    Payload = writer.ToString(),
                    CorrelationId = e.CorrelationId,
                    EventType = e.GetType().Name,
                    CreationDate = DateTime.Now
                };
            }
            return serialized;
        }

        private IVersionedEvent Deserialize(Event @event)
        {
            using (var reader = new StringReader(@event.Payload))
            {
                return (IVersionedEvent)this.serializer.Deserialize(reader);
            }
        }

        private string GetPartitionKey(Guid id)
        {
            return _sourceType + "_" + id.ToString();
        }
    }
}
