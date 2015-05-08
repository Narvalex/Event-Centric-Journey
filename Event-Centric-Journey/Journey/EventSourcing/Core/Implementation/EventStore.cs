using Journey.Database;
using Journey.Messaging;
using Journey.Serialization;
using Journey.Utils;
using Journey.Utils.SystemTime;
using Journey.Worker;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Journey.EventSourcing
{
    /// <summary>
    /// This is an extremely basic implementation of the event store (straw man), that is used only for running the application
    /// without the dependency to the Windows Azure Service Bus.
    /// It does check for event versions before committing, but is not transactional with the event bus nor resilient to connectivity errors or crashes.
    /// It does do snapshots for entities that implements <see cref="IMementoOriginator"/>.
    /// </summary>
    /// <remarks>
    /// <para>This is a basic implementation of the event store that could be optimized in the future.</para>
    /// <para>It supports caching of snapshot if the entity implements the <see cref="IMementoOriginator"/> interface. </para>
    /// </remarks>
    /// <typeparam name="T">The entity type to persist.</typeparam>
    public class EventStore<T> : EventStoreBase<T> where T : class, IEventSourced
    {
        private readonly IEventBus eventBus;
        private readonly ICommandBus commandBus;

        private readonly Func<EventStoreDbContext> contextFactory;
        private readonly Func<EventStoreDbContext> queryContextFactory;

        public EventStore(IEventBus eventBus, ICommandBus commandBus, ITextSerializer serializer, Func<EventStoreDbContext> contextFactory, IWorkerRoleTracer tracer, ISystemTime dateTime, ISnapshotProvider snapshoter)
            : base(tracer, serializer, dateTime, snapshoter)
        {
            this.eventBus = eventBus;
            this.commandBus = commandBus;

            this.contextFactory = contextFactory;
            this.queryContextFactory = () =>
            {
                var context = this.contextFactory.Invoke();
                context.Configuration.AutoDetectChangesEnabled = false;
                return context;
            };

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
                if (!cachedMemento.Item2.HasValue || cachedMemento.Item2.Value < this.dateTime.Now.AddMinutes(-30))
                {
                    using (var context = this.queryContextFactory.Invoke())
                    {
                        deserialized = context.Set<Event>()
                            .Where(x => x.SourceId == id && x.SourceType == _sourceType && x.Version > cachedMemento.Item1.Version)
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
                using (var context = this.queryContextFactory.Invoke())
                {
                    var deserialized = context.Set<Event>()
                        .Where(x => x.SourceId == id && x.SourceType == _sourceType)
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
WHERE SourceId = @SourceId
	AND SourceType = @SourceType)
e
", EventStoreDbContext.SchemaName, EventStoreDbContext.EventsTableName),
            new SqlParameter("@SourceId", eventSourced.Id),
            new SqlParameter("@SourceType", _sourceType))
            .FirstOrDefault() as int? ?? default(int);


            if (lastCommitedVersion + 1 != events[0].Version)
                throw new EventStoreConcurrencyException();
        }
    }
}
