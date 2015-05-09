using Journey.Messaging;
using Journey.Serialization;
using Journey.Utils;
using Journey.Utils.SystemTime;
using Journey.Worker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Journey.EventSourcing
{
    /// <summary>
    /// This is an extremely basic implementation of the event store (straw man), that is used only for running the application
    /// without the dependency to the Windows Azure Service Bus.
    /// It does check for event versions before committing, but is not transactional with the event bus nor resilient to connectivity errors or crashes.
    /// It does do snapshots for entities that implements <see cref="IMementoOriginator"/>.
    /// </summary>
    /// <typeparam name="T">The entity type to persist.</typeparam>
    public class InMemoryEventStore<T> : EventStoreBase<T> where T : class, IEventSourced
    {
        private readonly IInMemoryBus bus;
        private readonly EventStoreDbContext context;


        public InMemoryEventStore(IInMemoryBus bus, ITextSerializer serializer, EventStoreDbContext context, IWorkerRoleTracer tracer, ISystemTime dateTime, ISnapshotProvider snapshoter)
            : base(tracer, serializer, dateTime, snapshoter)
        {
            this.bus = bus;
            this.context = context;
        }

        public override T Find(Guid id)
        {
            var cachedMemento = this.getMementoFromCache(id);
            if (cachedMemento != null && cachedMemento.Item1 != null)
            {
                IEnumerable<IVersionedEvent> deserialized;
                deserialized = Enumerable.Empty<IVersionedEvent>();

                return this.originatorEntityFactory.Invoke(id, cachedMemento.Item1, deserialized);
            }
            else
            {
                var deserialized = this.context.Set<Event>()
                    .Local
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

            try
            {
                var eventsSet = this.context.Set<Event>();

                foreach (var e in events)
                {
                    // le pasamos el command id para que se serialice
                    e.CorrelationId = correlationId;
                    e.CreationDate = creationDate;
                    eventsSet.Add(this.Serialize(e));
                }

                this.bus.Publish(events);

                if (commands != null && commands.Count() > 0)
                    this.bus.Send(commands);
            }
            catch (Exception)
            {

                this.markCacheAsStale(eventSourced.Id);
                throw;
            }

            this.TakeSnapshotIfApplicable(eventSourced);
        }

        private void TakeSnapshotIfApplicable(T eventSourced)
        {
            this.cacheMementoIfApplicable.Invoke(eventSourced);

            var key = _sourceType + "_" + eventSourced.Id.ToString();
            var snapshot = this.getMementoFromCache(eventSourced.Id);

            var storedSnapshot = this.context
                .Snapshsots
                .Local
                .Where(x => x.PartitionKey == key)
                .FirstOrDefault();

            if (storedSnapshot == null)
                context.AddToUnityOfWork(this.SerializeSnapshot(key, snapshot.Item1, snapshot.Item2));
            else
            {
                storedSnapshot.Memento = this.SerializeSnapshot(key, snapshot.Item1, snapshot.Item2).Memento;
                storedSnapshot.LastUpdateTime = snapshot.Item2;
                context.AddToUnityOfWork(storedSnapshot);
            }
        }

        private RollingSnapshot SerializeSnapshot(string partitionKey, IMemento memento, DateTime? lastUpdateTime)
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
