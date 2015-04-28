using System.Data.Entity;

namespace Journey.EventSourcing
{
    /// <summary>
    /// Used by <see cref="SqlEventSourcedRepository{T}"/>.
    /// </summary>
    public class EventStoreDbContext : DbContext
    {
        public const string SchemaName = "EventStore";
        public const string TableName = "Events";

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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>().HasKey(x => new { x.AggregateId, x.AggregateType, x.Version }).ToTable(TableName, SchemaName);
        }
    }
}
