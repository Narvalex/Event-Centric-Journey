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
        { }

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
                            .TryGetValue(SimpleInventarioTables.ResumenDeAnimalesDeTodosLosPeriodos)
                            .TableName,
                        this.TablesInfo.TryGetValue(SimpleInventarioTables.ResumenDeAnimalesDeTodosLosPeriodos)
                            .SchemaName)
                .HasKey(x => x.Periodo);
        }

        protected override void OnRegisteringTableInfo()
        {
            base.OnRegisteringTableInfo();

            base.RegisterTableInfo(
                SimpleInventarioTables.ResumenDeAnimalesDeTodosLosPeriodos,
                SimpleInventarioTables.ResumenDeAnimalesDeTodosLosPeriodos,
                SimpleInventarioSchemas.ReadModel);
        }
    }

    public static class SimpleInventarioTables
    {
        public const string ResumenDeAnimalesDeTodosLosPeriodos = "ResumenDeAnimalesDeTodosLosPeriodos";
    }

    public static class SimpleInventarioSchemas
    {
        public const string ReadModel = "ReadModel";
    }
}
