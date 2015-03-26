using Journey.Database;
using Journey.Messaging;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils;
using Journey.Worker;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace Journey.EventSourcing.ReadModeling
{
    public abstract class ReadModelBuilder
    {
        protected readonly string connectionString;
        protected readonly IWorkerRoleTracer tracer;
        private readonly ITextSerializer serializer;
        protected readonly IEventDispatcher eventDispatcher;

        public ReadModelBuilder(string connectionString, IWorkerRoleTracer tracer)
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
            this.connectionString = connectionString;
            this.serializer = new JsonTextSerializer();
            this.tracer = tracer;
            this.EventsCount = this.GetEventsCount();
            this.eventDispatcher = new SynchronousEventDispatcher(this.tracer);

            //this.container = this.CreateContainer();
            this.RegisterAllGenerators();
        }

        public int EventsCount { get; private set; }

        public void BuildReadModel()
        {
            this.InitializeDatabase();
            this.MaterializeEvents();
        }

        private void MaterializeEvents()
        {
            DateTime startTime;
            this.tracer.Notify("====> Opening connection...");

            using (var context = new EventStoreDbContext(this.connectionString))
            {
                this.tracer.Notify("====> Loading events...");
                var events = context.Set<Event>()
                    .OrderBy(e => e.CreationDate)
                    .AsEnumerable()
                    .Select(this.Deserialize)
                    .AsCachedAnyEnumerable();

                // Setting the real datetime
                startTime = DateTime.Now;
                this.tracer.Notify(string.Empty);
                this.tracer.Notify(string.Format("Starting at {0}", startTime.ToString()));

                if (events.Any())
                {
                    this.tracer.Notify("====> Dispatching events...");
                    foreach (var e in events)
                    {
                        var @event = (IEvent)e;
                        this.eventDispatcher.DispatchMessage(@event, null, @event.SourceId.ToString(), "");
                    }
                    this.tracer.Notify("====> All events where dispatched.");
                }
            }

            this.tracer.Notify("====> Hitting the Database");
            var rowsAffected = this.Commit();

            #region Ending Message

            var endTime = DateTime.Now;
            var timeElapsed = endTime - startTime;
            var eventSpeed = this.EventsCount / timeElapsed.TotalSeconds;
            var dbSpeed = rowsAffected / timeElapsed.TotalSeconds;

            this.tracer.Notify(string.Empty);
            this.tracer.Notify(string.Format("Finished at {0}", endTime.ToString()));
            this.tracer.Notify(string.Empty);
            this.tracer.Notify(string.Format(
                "Time elapsed: {0} days, {1} hours, {2} minutes, {3} seconds",
                timeElapsed.TotalDays.ToString(),
                timeElapsed.TotalHours.ToString(),
                timeElapsed.TotalMinutes.ToString(),
                timeElapsed.TotalSeconds.ToString()));
                        
            this.tracer.Notify(string.Empty);
            this.tracer.Notify(string.Format("Events count: {0}", this.EventsCount));
            this.tracer.Notify(string.Format("Average event processing speed: {0} events per second.", eventSpeed.ToString()));
            this.tracer.Notify(string.Empty);
            this.tracer.Notify(string.Format("Number of rows affected: {0} rows", rowsAffected));
            this.tracer.Notify(string.Format("Database speed: {0} rows per second.", dbSpeed));

            #endregion
        }

        protected abstract void RegisterAllGenerators();

        protected abstract void InitializeDatabase();

        /// <summary>
        /// Commits the denormalization process to the underlying database.
        /// </summary>
        /// <returns>The number of objects written to the underlying database</returns>
        protected abstract int Commit();

        private ITraceableVersionedEvent Deserialize(Event @event)
        {
            using (var reader = new StringReader(@event.Payload))
            {
                return (ITraceableVersionedEvent)this.serializer.Deserialize(reader);
            }
        }

        private int GetEventsCount()
        {
            var sql = new SqlCommandWrapper(this.connectionString);
            return sql.ExecuteReader(@"
                        select count(*) as RwCnt 
                        from EventStore.Events 
                        ", r => r.SafeGetInt32(0))
                         .FirstOrDefault();
        }
    }
}
