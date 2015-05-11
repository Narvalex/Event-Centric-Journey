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
    /// <summary>
    /// El Read Model Builder construye todo el read model, 
    /// desde el primer hasta el ultimo evento.
    /// </summary>
    public abstract class OldReadModelRebuilder
    {
        protected readonly string connectionString;
        protected readonly ITracer tracer;
        private readonly ITextSerializer serializer;
        protected readonly IEventDispatcher eventDispatcher;

        public OldReadModelRebuilder(string connectionString, ITracer tracer)
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
            this.tracer.TraceAsync("====> Opening connection...");

            using (var context = new EventStoreDbContext(this.connectionString))
            {
                this.tracer.TraceAsync("====> Loading events...");
                var events = context.Set<Event>()
                    .OrderBy(e => e.CreationDate)
                    .AsEnumerable()
                    .Select(this.Deserialize)
                    .AsCachedAnyEnumerable();

                // Setting the real datetime
                startTime = DateTime.Now;
                this.tracer.TraceAsync(string.Empty);
                this.tracer.TraceAsync(string.Format("Starting at {0}", startTime.ToString()));

                if (events.Any())
                {
                    this.tracer.TraceAsync("====> Dispatching events...");
                    foreach (var e in events)
                    {
                        var @event = (IEvent)e;
                        this.eventDispatcher.DispatchMessage(@event, null, @event.SourceId.ToString(), "");
                    }
                    this.tracer.TraceAsync("====> All events where dispatched.");
                }
            }

            this.tracer.TraceAsync("====> Hitting the Database");
            var rowsAffected = this.Commit();

            #region Ending Message

            var endTime = DateTime.Now;
            var timeElapsed = endTime - startTime;
            var eventSpeed = this.EventsCount / timeElapsed.TotalSeconds;
            var dbSpeed = rowsAffected / timeElapsed.TotalSeconds;

            this.tracer.TraceAsync(string.Empty);
            this.tracer.TraceAsync(string.Format("Finished at {0}", endTime.ToString()));
            this.tracer.TraceAsync(string.Empty);
            this.tracer.TraceAsync(string.Format(
                "Time elapsed: {0} days, {1} hours, {2} minutes, {3} seconds",
                timeElapsed.TotalDays.ToString(),
                timeElapsed.TotalHours.ToString(),
                timeElapsed.TotalMinutes.ToString(),
                timeElapsed.TotalSeconds.ToString()));
                        
            this.tracer.TraceAsync(string.Empty);
            this.tracer.TraceAsync(string.Format("Events count: {0}", this.EventsCount));
            this.tracer.TraceAsync(string.Format("Average event processing speed: {0} events per second.", eventSpeed.ToString()));
            this.tracer.TraceAsync(string.Empty);
            this.tracer.TraceAsync(string.Format("Number of rows affected: {0} rows", rowsAffected));
            this.tracer.TraceAsync(string.Format("Database speed: {0} rows per second.", dbSpeed));

            #endregion
        }

        protected abstract void RegisterAllGenerators();

        protected abstract void InitializeDatabase();

        /// <summary>
        /// Commits the denormalization process to the underlying database.
        /// </summary>
        /// <returns>The number of objects written to the underlying database</returns>
        protected abstract int Commit();

        private IVersionedEvent Deserialize(Event @event)
        {
            using (var reader = new StringReader(@event.Payload))
            {
                return (IVersionedEvent)this.serializer.Deserialize(reader);
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
