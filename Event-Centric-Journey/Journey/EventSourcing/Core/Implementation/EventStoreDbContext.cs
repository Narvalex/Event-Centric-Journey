using Journey.Messaging.Logging;
using System.Data.Entity;

namespace Journey.EventSourcing
{
    /// <summary>
    /// Used by <see cref="SqlEventSourcedRepository{T}"/>.
    /// </summary>
    public class EventStoreDbContext : DbContext
    {
        internal const string EventStoreSchemaName = "EventStore";
        internal const string EventsTableName = "Events";
        private const string SnapshotsTableName = "Snapshots";

        private const string LogSchemaName = "MessageLog";
        private const string LogTableName = "Messages";

        static EventStoreDbContext()
        {
            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
        }

        public EventStoreDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        { }

        public EventStoreDbContext()
            : base("Name=defaultConnection")
        { }

        public IDbSet<RollingSnapshot> Snapshsots { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>()
                .HasKey(x => new { SourceId = x.SourceId, SourceType = x.SourceType, x.Version })
                .ToTable(EventsTableName, EventStoreSchemaName);

            modelBuilder.Entity<RollingSnapshot>()
                .HasKey(x => x.PartitionKey)
                .ToTable(SnapshotsTableName, EventStoreSchemaName);

            modelBuilder.Entity<MessageLogEntity>().ToTable(LogTableName, LogSchemaName);
            modelBuilder.Entity<MessageLogEntity>().HasKey(m => m.Id);
        }
    }
}
