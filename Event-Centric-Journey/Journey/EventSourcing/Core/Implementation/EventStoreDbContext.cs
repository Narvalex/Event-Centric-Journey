using System.Data.Entity;

namespace Journey.EventSourcing
{
    /// <summary>
    /// Used by <see cref="SqlEventSourcedRepository{T}"/>.
    /// </summary>
    public class EventStoreDbContext : DbContext
    {
        public const string SchemaName = "EventStore";
        public const string EventsTableName = "Events";
        public const string SnapshotsTableName = "Snapshots";

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
                .ToTable(EventsTableName, SchemaName);

            modelBuilder.Entity<RollingSnapshot>()
                .HasKey(x => x.PartitionKey)
                .ToTable(SnapshotsTableName, SchemaName);
        }
    }
}
