using Journey.Database;
using Journey.Messaging;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils;
using System;
using System.Data.Entity;
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
                try
                {
                    TransientFaultHandlingDbConfiguration.SuspendExecutionStrategy = true;

                    using (var dbContextTransaction = this.readModelContext.Database.BeginTransaction())
                    {
                        try
                        {
                            this.ClearDatabase();

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

                            this.readModelContext.SaveChanges();

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
        }

        private IVersionedEvent Deserialize(Event @event)
        {
            using (var reader = new StringReader(@event.Payload))
            {
                return (IVersionedEvent)this.serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// URL: http://stackoverflow.com/questions/6089403/delete-all-entities-in-entity-framework
        /// </summary>
        private void ClearDatabase()
        {
            // cuenta algo
            var result = 0;
            foreach (var tableInfo in this.readModelContext.TablesInfo)
            {
                result += this.readModelContext.Database
                       .ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, string.Format(@"
                            DELETE FROM [{0}].[{1}]",
                            tableInfo.Value.SchemaName,
                            tableInfo.Value.TableName));

                if (tableInfo.Value.HasIdentityColumn)
                {
                    this.readModelContext.Database
                        .ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, string.Format("DBCC CHECKIDENT ('[{0}].[{1}]', RESEED, 0)",
                        tableInfo.Value.SchemaName,
                        tableInfo.Value.TableName));
                }
            }
        }
    }
}
