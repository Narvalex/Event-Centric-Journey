using Journey.Utils;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Journey.EventSourcing.ReadModeling
{
    public class ReadModelDbContext : DbContext
    {
        private Dictionary<string, TableInfo> tablesInfo = new Dictionary<string, TableInfo>();

        static ReadModelDbContext()
        {
            System.Data.Entity.Database.SetInitializer<ReadModelDbContext>(null);
        }

        public ReadModelDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            this.OnRegisteringTableInfo();
        }

        public ReadModelDbContext()
            : base("Name=defaultConnection")
        {
            this.OnRegisteringTableInfo();
        }

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

            modelBuilder
            .Entity<ProjectedEvent>()
            .HasKey(l => new { l.AggregateId, l.Version, l.AggregateType })
            .ToTable(
                this.tablesInfo.TryGetValue(ReadModelDbContextTables.ReadModelingEvents).TableName,
                this.tablesInfo.TryGetValue(ReadModelDbContextTables.ReadModelingEvents).SchemaName);
        }

        public IDbSet<ProjectedEvent> ReadModelingEvents { get; set; }

        public Dictionary<string, TableInfo> TablesInfo 
        { 
            get { return this.tablesInfo; }
        }

        protected void RegisterTableInfo(string dbSetName, string tableName, string schemaName)
        {
            this.tablesInfo.Add(dbSetName, new TableInfo(tableName, schemaName, false));
        }

        protected virtual void OnRegisteringTableInfo()
        {
            this.RegisterTableInfo(
                ReadModelDbContextTables.ReadModelingEvents,
                ReadModelDbContextTables.ReadModelingEvents,
                ReadModelDbContextSchemas.SubscriptionLog);
        }
    }

    public class ReadModelDbContextTables
    {
        public const string ReadModelingEvents = "ReadModelingEvents";
    }

    public class ReadModelDbContextSchemas
    {
        public const string SubscriptionLog = "SubscriptionLog";
    }
}
