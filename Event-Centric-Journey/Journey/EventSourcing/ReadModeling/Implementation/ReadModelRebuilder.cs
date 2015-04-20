using Journey.Messaging;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils;
using Journey.Worker;
using System;
using System.IO;
using System.Linq;

namespace Journey.EventSourcing.ReadModeling.Implementation
{
    public class ReadModelRebuilder : IReadModelRebuilder
    {
        private readonly IWorkerRole worker;
        private readonly ITextSerializer serializer;
        private readonly IEventDispatcher eventDispatcher;
        private readonly Func<EventStoreDbContext> storeContextFactory;
        private readonly ReadModelDbContext readModelContext;

        public ReadModelRebuilder(IWorkerRole worker, Func<EventStoreDbContext> storeContextFactory, ITextSerializer serializer, IEventDispatcher synchronousEventDispatcher, ReadModelDbContext readModelContext)
        {
            this.worker = worker;
            this.storeContextFactory = storeContextFactory;
            this.serializer = serializer;
            this.eventDispatcher = synchronousEventDispatcher;
            this.readModelContext = readModelContext;
        }

        public void Rebuild()
        {
            // paramos el worker role
            // TODO: verificar que efectivamente esta parado.
            this.worker.Stop();

            using (var context = this.storeContextFactory.Invoke())
            {
                var events = context.Set<Event>()
                    .OrderBy(e => e.CreationDate)
                    .AsEnumerable()
                    .Select(this.Deserialize)
                    .AsCachedAnyEnumerable();

                if (events.Any())
                {
                    foreach (var e in events)
                    {
                        var @event = (IEvent)e;
                        this.eventDispatcher.DispatchMessage(@event, null, @event.SourceId.ToString(), "");
                    }
                }
            }
            // TODO: clean DbContext, in a transaction.
            this.readModelContext.SaveChanges();
        }

        private IVersionedEvent Deserialize(Event @event)
        {
            using (var reader = new StringReader(@event.Payload))
            {
                return (IVersionedEvent)this.serializer.Deserialize(reader);
            }
        }
    }
}
