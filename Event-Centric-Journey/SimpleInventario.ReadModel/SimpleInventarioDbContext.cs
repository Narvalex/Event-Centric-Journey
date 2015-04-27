using Journey.EventSourcing.ReadModeling;
using Journey.Utils;
using SimpleInventario.ReadModel.Entities;
using System.Data.Entity;

namespace SimpleInventario.ReadModel
{
    /// <summary>
    /// El Read Model debe ser global
    /// </summary>
    public class SimpleInventarioDbContext : ReadModelDbContext
    {
        #region Constructors
        static SimpleInventarioDbContext()
        {
            System.Data.Entity.Database.SetInitializer<SimpleInventarioDbContext>(null);
        }

        public SimpleInventarioDbContext(string connectionString)
            : base(connectionString)
        {
            base.RegisterTableInfo(SimpleInventarioDbContextTables.ResumenDeAnimalesDeTodosLosPeriodos, SimpleInventarioDbContextTables.ResumenDeAnimalesDeTodosLosPeriodos, SimpleInventarioDbContextSchemas.Sireport);
        }

        public SimpleInventarioDbContext()
            : base("Name=defaultConnection")
        { } 
        #endregion

        public IDbSet<CantidadDeAnimalesDeUnPeriodo> ResumenDeAnimalesDeTodosLosPeriodos { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CantidadDeAnimalesDeUnPeriodo>()
                .ToTable(this.TablesInfo
                            .TryGetValue(SimpleInventarioDbContextTables.ResumenDeAnimalesDeTodosLosPeriodos)
                            .TableName,
                        this.TablesInfo.TryGetValue(SimpleInventarioDbContextTables.ResumenDeAnimalesDeTodosLosPeriodos)
                            .SchemaName)
                .HasKey(x => x.Periodo);
        }
    }

    public static class SimpleInventarioDbContextTables
    {
        public const string ResumenDeAnimalesDeTodosLosPeriodos = "ResumenDeAnimalesDeTodosLosPeriodos";
    }

    public static class SimpleInventarioDbContextSchemas
    {
        public const string Sireport = "Sireport";
    }
}
