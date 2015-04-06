using Journey.EventSourcing.ReadModeling;
using System.Data.Entity;

namespace Journey.Tests.Integration.EventSourcing.ReadModeling
{
    public class ItemReadModelDbContext : ReadModelDbContext
    {
        public ItemReadModelDbContext(string connectionString)
            : base(connectionString)
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>()
                .ToTable("Items", "Domain")
                .HasKey(i => i.UnidentifiableId);
        }

        public IDbSet<Item> Items { get; set; }
    }

    public class Item
    {
        public int UnidentifiableId { get; set; }
        public string Name { get; set; }
    }
}
