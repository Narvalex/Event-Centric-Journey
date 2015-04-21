using Journey.Messaging;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils;
using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;

namespace Journey.EventSourcing.ReadModeling.Implementation
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
            var objectContext = ((IObjectContextAdapter)this.readModelContext).ObjectContext;
            var entities = 
                objectContext
                .MetadataWorkspace
                .GetEntityContainer(objectContext.DefaultContainerName, DataSpace.CSpace)
                .BaseEntitySets;
            var method = objectContext.GetType().GetMethods().First(x => x.Name == "CreateObjectSet");
            var objectSets =
                entities
                .Select(x => method.MakeGenericMethod(Type.GetType(x.ElementType.FullName)))
                .Select(x => x.Invoke(objectContext, null));
            var tablesNames =
                objectSets
                .Select(objectSet =>
                    (objectSet.GetType().GetProperty("EntitySet").GetValue(objectSet, null) as EntitySet).Name).ToList();

            foreach (var tableName in tablesNames)
                this.readModelContext.Database.ExecuteSqlCommand(
                    string.Format("DELETE FROM {0}", tableName));
        }
    }
}
