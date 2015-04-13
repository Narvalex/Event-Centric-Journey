using System.Data.Entity;
using System.Linq;

namespace Journey.EventSourcing.ReadModeling
{
    public class ReadModelDbContext : DbContext
    {
        static ReadModelDbContext()
        {
            System.Data.Entity.Database.SetInitializer<ReadModelDbContext>(null);
        }

        public ReadModelDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        { }

        public ReadModelDbContext()
            : base("Name=defaultConnection")
        { }

        public IQueryable<T> Query<T>() where T : class
        {
            return this.Set<T>();
        }

        public void AddToUnitOfWork<T>(T entity) where T : class
        {
            var entry = this.Entry(entity);

            if (entry.State == EntityState.Detached)
                this.Set<T>().Add(entity);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProjectedEvent>()
                .ToTable("ReadModeling", "SubscriptionLog")
                .HasKey(l => new { l.AggregateId, l.Version, l.AggregateType });
        }

        public IDbSet<ProjectedEvent> ProjectedEvents { get; set; }
    }
}
