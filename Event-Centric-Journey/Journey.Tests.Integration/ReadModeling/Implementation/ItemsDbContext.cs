using Journey.EventSourcing.ReadModeling;
using System.Data.Entity;
using System.Linq;

namespace Journey.Tests.Integration.ReadModeling.Implementation
{
    /// <summary>
    /// A repository stored in a database for the views.
    /// </summary>
    public class ItemsDbContext : ReadModelDbContext
    {
        public const string SchemaName = "Items";

        public ItemsDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        { }

        public DbSet<ItemView> Items { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ItemView>().ToTable("Items", SchemaName).HasKey(i => i.ItemId);
        }

        public T Find<T>(int id) where T : class
        {
            return this.Set<T>().Find(id);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return this.Set<T>();
        }

        public void Save<T>(T entity) where T : class
        {
            var entry = this.Entry(entity);

            if (entry.State == EntityState.Detached)
                this.Set<T>().Add(entity);

            this.SaveChanges();
        }
    }
}
