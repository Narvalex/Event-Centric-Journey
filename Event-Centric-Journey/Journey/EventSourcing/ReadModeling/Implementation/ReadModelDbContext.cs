using Journey.Utils;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Journey.EventSourcing.ReadModeling
{
    /// <summary>
    /// Mandatory convention: use the same table name for the propertie of the DbSet in order to have the same info.948576
    /// </summary>
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
            this.RegisterTableInfo(
                ReadModelDbContextTables.ReadModeling,
                ReadModelDbContextTables.ReadModeling,
                ReadModelDbContextSchemas.SubscriptionLog);
        }

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

            modelBuilder
            .Entity<ProjectedEvent>()
            .HasKey(l => new { l.AggregateId, l.Version, l.AggregateType })
            .ToTable(
                this.tablesInfo.TryGetValue(ReadModelDbContextTables.ReadModeling).TableName,
                this.tablesInfo.TryGetValue(ReadModelDbContextTables.ReadModeling).SchemaName);
        }

        public IDbSet<ProjectedEvent> ReadModeling { get; set; }

        public Dictionary<string, TableInfo> TablesInfo 
        { 
            get { return this.tablesInfo; }
        }

        protected void RegisterTableInfo(string dbSetName, string tableName, string schemaName)
        {
            this.tablesInfo.Add(dbSetName, new TableInfo(tableName, schemaName, false));
        }
    }

    public class ReadModelDbContextTables
    {
        public const string ReadModeling = "ReadModeling";
    }

    public class ReadModelDbContextSchemas
    {
        public const string SubscriptionLog = "SubscriptionLog";
    }
}
