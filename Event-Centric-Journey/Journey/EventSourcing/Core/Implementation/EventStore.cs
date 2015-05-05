using Journey.Database;
using Journey.Messaging;
using Journey.Serialization;
using Journey.Utils;
using Journey.Utils.SystemDateTime;
using Journey.Worker;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    public class EventStore<T> : EventStoreBase<T> where T : class, IEventSourced
    {
        private readonly IEventBus eventBus;
        private readonly ICommandBus commandBus;
        private readonly Func<EventStoreDbContext> contextFactory;
        private readonly Action<T> cacheMementoIfApplicable;
        private readonly IInMemoryRollingSnapshotProvider cache;
        private readonly Func<Guid, Tuple<IMemento, DateTime?>> getMementoFromCache;
        private readonly Action<Guid> markCacheAsStale;
        private readonly Func<Guid, IMemento, IEnumerable<IVersionedEvent>, T> originatorEntityFactory;

        public EventStore(IEventBus eventBus, ICommandBus commandBus, ITextSerializer serializer, Func<EventStoreDbContext> contextFactory, IInMemoryRollingSnapshotProvider cache, IWorkerRoleTracer tracer, ISystemDateTime dateTime)
            : base(tracer, serializer, dateTime)
        {
            this.eventBus = eventBus;
            this.commandBus = commandBus;
            this.contextFactory = contextFactory;
            this.cache = cache;

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
                        new Tuple<IMemento, DateTime?>(memento, this.dateTime.Now),
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

            if (!typeof(ISqlBus).IsAssignableFrom(this.eventBus.GetType()))
                throw new InvalidCastException("El eventBus debe implementar ISqlBus para ser transaccional con el EventStore");

            if (!typeof(ISqlBus).IsAssignableFrom(this.commandBus.GetType()))
                throw new InvalidCastException("El commandBus debe implementar ISqlBus para ser transaccional con el EventStore");
        }

        public override T Find(Guid id)
        {
            var cachedMemento = this.getMementoFromCache(id);
            if (cachedMemento != null && cachedMemento.Item1 != null)
            {
                // NOTE: if we had a guarantee that this is running in a single process, there is
                // no need to check if there are new events after the cached version.
                IEnumerable<IVersionedEvent> deserialized;
                if (!cachedMemento.Item2.HasValue || cachedMemento.Item2.Value < this.dateTime.Now.AddSeconds(-1))
                {
                    using (var context = this.contextFactory.Invoke())
                    {
                        deserialized = context.Set<Event>()
                            .Where(x => x.AggregateId == id && x.AggregateType == _sourceType && x.Version > cachedMemento.Item1.Version)
                            .OrderBy(x => x.Version)
                            .AsEnumerable()
                            .Select(this.Deserialize)
                            .AsCachedAnyEnumerable();

                        if (deserialized.Any())
                            return entityFactory.Invoke(id, deserialized);
                    }
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
                using (var context = this.contextFactory.Invoke())
                {
                    var deserialized = context.Set<Event>()
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
        }

        public override void Save(T eventSourced, Guid correlationId, DateTime creationDate)
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

            using (var context = this.contextFactory.Invoke())
            {
                try
                {
                    TransientFaultHandlingDbConfiguration.SuspendExecutionStrategy = true;

                    using (var dbContextTransaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            var eventsSet = context.Set<Event>();

                            foreach (var e in events)
                            {
                                // le pasamos el command id para que se serialice
                                e.CorrelationId = correlationId;
                                // la fecha en que se creó el evento
                                e.CreationDate = creationDate;
                                eventsSet.Add(this.Serialize(e));
                            }

                            this.GuaranteeIncrementalEventVersionStoring(eventSourced, events, context);



                            var correlationIdString = correlationId.ToString();
                            this.eventBus.Publish(events.Select(e => new Envelope<IEvent>(e) { CorrelationId = correlationIdString }), context);

                            if (commands != null && commands.Count() > 0)
                                this.commandBus.Send(commands.Select(c => new Envelope<ICommand>(c) { CorrelationId = correlationIdString }), context);

                            context.SaveChanges();

                            dbContextTransaction.Commit();
                        }
                        catch (Exception)
                        {
                            try
                            {
                                dbContextTransaction.Rollback();
                            }
                            catch (Exception)
                            { }

                            this.markCacheAsStale(eventSourced.Id);
                            throw;
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    TransientFaultHandlingDbConfiguration.SuspendExecutionStrategy = false;
                }
            }



            this.cacheMementoIfApplicable.Invoke(eventSourced);
        }

        /// <summary>
        /// Guarantee that only incremental versions of the event are stored
        /// </summary>
        private void GuaranteeIncrementalEventVersionStoring(T eventSourced, IVersionedEvent[] events, EventStoreDbContext context)
        {
            // Checking if this is the first ever event for this aggregate
            // Another option could be use the T-SQL method 'ISNULL'.
            // For expample: "SELECT LastVersion = ISNULL(Max([e].[Version]), -1)"
            var lastCommitedVersion = context.Database.SqlQuery<int?>(
                string.Format(@"
SELECT LastVersion = Max([e].[Version])
FROM 
(SELECT [Version] 
FROM [{0}].[{1}] WITH (READPAST)
WHERE AggregateId = @AggregateId
	AND AggregateType = @AggregateType)
e
", EventStoreDbContext.SchemaName, EventStoreDbContext.TableName),
            new SqlParameter("@AggregateId", eventSourced.Id),
            new SqlParameter("@AggregateType", _sourceType))
            .FirstOrDefault() as int? ?? default(int);


            if (lastCommitedVersion + 1 != events[0].Version)
                throw new EventStoreConcurrencyException();
        }
    }
}
