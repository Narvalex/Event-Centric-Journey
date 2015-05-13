using Journey.Database;
using Journey.EventSourcing.RebuildPerfCounting;
using Journey.Messaging;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils;
using System;
using System.IO;
using System.Linq;

namespace Journey.EventSourcing.ReadModeling
{
    public class ReadModelRebuilderEngine<T> : IReadModelRebuilderEngine<T> where T : ReadModelDbContext
    {
        private readonly ITextSerializer serializer;
        private readonly IEventDispatcher eventDispatcher;
        private readonly Func<EventStoreDbContext> storeContextFactory;
        private readonly T readModelContext;
        private readonly IRebuilderPerfCounter perfCounter;

        public ReadModelRebuilderEngine(Func<EventStoreDbContext> storeContextFactory, ITextSerializer serializer, IEventDispatcher synchronousEventDispatcher, T readModelContext, IRebuilderPerfCounter perfCounter)
        {
            this.storeContextFactory = storeContextFactory;
            this.serializer = serializer;
            this.eventDispatcher = synchronousEventDispatcher;
            this.readModelContext = readModelContext;
            this.perfCounter = perfCounter;
        }

        /// <summary>
        /// Rebuild the read model. If executed in process must only be fired AFTER the worker 
        /// role stops processing messages.
        /// </summary>
        public void Rebuild()
        {
            this.perfCounter.OnStartingRebuildProcess(this.GetEventsCount());
            this.perfCounter.OnOpeningDbConnectionAndCleaning();

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

                            this.perfCounter.OnDbConnectionOpenedAndCleansed();

                            var events = context.Set<Event>()
                                .OrderBy(e => e.SourceId)
                                .ThenBy(e => e.SourceType)
                                .ThenBy(e => e.Version)
                                .AsEnumerable()
                                .Select(this.Deserialize)
                                .AsCachedAnyEnumerable();

                            if (events.Any())
                            {
                                this.perfCounter.OnStartingStreamProcessing();

                                foreach (var e in events)
                                {
                                    var @event = (IEvent)e;
                                    this.eventDispatcher.DispatchMessage(@event, null, @event.SourceId.ToString(), "");
                                }

                                this.perfCounter.OnStreamProcessingFinished();
                            }

                            this.perfCounter.OnStartingCommitting();

                            var rowsAffected = this.readModelContext.SaveChanges();

                            dbContextTransaction.Commit();

                            this.perfCounter.OnCommitted(rowsAffected);
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

        private int GetEventsCount()
        {
            string connectionString;
            using (var context = this.storeContextFactory.Invoke())
            {
                connectionString = context.Database.Connection.ConnectionString;
            }

            var sql = new SqlCommandWrapper(connectionString);
            return sql.ExecuteReader(@"
                        select count(*) as RwCnt 
                        from EventStore.Events 
                        ", r => r.SafeGetInt32(0))
                         .FirstOrDefault();
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
                       .ExecuteSqlCommand(string.Format(@"
                            DELETE FROM [{0}].[{1}]",
                            tableInfo.Value.SchemaName,
                            tableInfo.Value.TableName));

                if (tableInfo.Value.HasIdentityColumn)
                {
                    this.readModelContext.Database
                        .ExecuteSqlCommand(string.Format("DBCC CHECKIDENT ('[{0}].[{1}]', RESEED, 0)",
                        tableInfo.Value.SchemaName,
                        tableInfo.Value.TableName));
                }
            }
        }
    }
}
