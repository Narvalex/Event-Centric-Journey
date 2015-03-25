using Journey.Database;
using System.Data.Entity;

namespace Journey.Messaging.Logging
{
    [DbConfigurationType(typeof(TransientFaultHandlingDbConfiguration))]
    public class MessageLogDbContext : DbContext
    {
        public const string SchemaName = "MessageLog";

        static MessageLogDbContext()
        {
            System.Data.Entity.Database.SetInitializer<MessageLogDbContext>(null);
        }

        public MessageLogDbContext()
            : base("Name=defaultConnection")
        { }

        public MessageLogDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MessageLogEntity>().ToTable("Messages", SchemaName);
            modelBuilder.Entity<MessageLogEntity>().HasKey(m => m.Id);
        }
    }
}
