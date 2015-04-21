using Journey.Messaging;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils;
using System;
using System.IO;
using System.Linq;

namespace Journey.EventSourcing.ReadModeling
{
    public class ReadModelRebuilder<T> : IReadModelRebuilder<T> where T : ReadModelDbContext
    {
        private readonly ITextSerializer serializer;
        private readonly IEventDispatcher eventDispatcher;
        private readonly Func<EventStoreDbContext> storeContextFactory;
        private readonly T readModelContext;

        public ReadModelRebuilder(Func<EventStoreDbContext> storeContextFactory, ITextSerializer serializer, IEventDispatcher synchronousEventDispatcher, T readModelContext)
        {
            this.storeContextFactory = storeContextFactory;
            this.serializer = serializer;
            this.eventDispatcher = synchronousEventDispatcher;
            this.readModelContext = readModelContext;
        }

        /// <summary>
        /// Rebuild the read model. If executed in process must only be fired AFTER the worker 
        /// role stops processing messages.
        /// </summary>
        public void Rebuild()
        {
            // paramos el worker role
            // TODO: verificar que efectivamente esta parado.

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

            this.ClearDatabase();            

            this.readModelContext.SaveChanges();
        }

        private IVersionedEvent Deserialize(Event @event)
        {
            using (var reader = new StringReader(@event.Payload))
            {
                return (IVersionedEvent)this.serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Base on an answer in Stackoverflow:
        /// URL: http://stackoverflow.com/questions/6089403/delete-all-entities-in-entity-framework
        /// </summary>
        private void ClearDatabase()
        {
//            var script = @"
//            SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
//            WHERE TABLE_TYPE = 'BASE TABLE' 
//            AND TABLE_NAME NOT LIKE '%Migration%'";

//            var tableNames = this.readModelContext.Database.SqlQuery<string>(script).ToList();
//            foreach (var tableName in tableNames)
//                this.readModelContext.Database.ExecuteSqlCommand(
//                    string.Format("DELETE FROM {0}", tableName));
        }
    }
}
