using Journey.EventSourcing.ReadModeling;
using SimpleInventario.ReadModel.Entities;
using System.Data.Entity;

namespace SimpleInventario.ReadModel
{
    /// <summary>
    /// El Read Model debe ser global
    /// </summary>
    public class SimpleInventarioDbContext : ReadModelDbContext
    {
        private const string SimpleInventarioReportSchema = "sireport";

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
                .ToTable("ResumenDeAnimalesDeTodosLosPeriodos", SimpleInventarioReportSchema)
                .HasKey(x => x.Periodo);
        }
    }
}
