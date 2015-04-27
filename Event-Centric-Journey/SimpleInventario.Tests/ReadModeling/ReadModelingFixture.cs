using Journey.Database;
using Journey.EventSourcing.ReadModeling;
using Journey.Utils.Guids;
using Journey.Worker;
using SimpleInventario.ReadModel;
using SimpleInventario.ReadModeling;
using SimpleInventario.Reporting.Events;
using System;
using System.Data.Entity;
using System.Linq;
using Xunit;

namespace SimpleInventario.Tests.ReadModeling.ReadModelingFixture
{
    public class GIVEN_generator_and_read_model : IDisposable
    {
        protected string connectionString;
        protected Func<SimpleInventarioDbContext> contextFactory;
        protected SimpleInventarioReadModelGenerator sut;

        public GIVEN_generator_and_read_model()
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());
            var dbName = "ReadModelingFixture";
            this.connectionString = System.Data.Entity.Database.DefaultConnectionFactory.CreateConnection(dbName).ConnectionString;
            this.contextFactory = () => new SimpleInventarioDbContext(this.connectionString);

            var generator = new ReadModelGeneratorEngine<SimpleInventarioDbContext>(
                this.contextFactory, new ConsoleWorkerRoleTracer());

            this.sut = new SimpleInventarioReadModelGenerator(generator);

            using (var context = this.contextFactory.Invoke())
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }
        }

        public void Dispose()
        {
            SqlCommandWrapper.DropDatabase(this.connectionString);
        }
    }

    public class GIVEN_incoming_events : GIVEN_generator_and_read_model
    {
        [Fact]
        public void CUANDO_se_actualiza_resumen_de_animales_por_periodo_ENTONCES_se_proyecta()
        {
            var correlationId = SequentialGuid.GenerateNewGuid();

            var e = new SeActualizoResumenDeAnimalesPorPeriodo
            {
                AggregateType = "TestAggregate",
                CantidadDeAnimales = 5,
                CorrelationId = correlationId,
                Periodo = 2015,
                SourceId = Guid.Empty,
                Version = 1
            };

            this.sut.Handle(e);

            using (var context = this.contextFactory.Invoke())
            {
                var resumen = context.ResumenDeAnimalesDeTodosLosPeriodos
                    .Where(x => x.Periodo == e.Periodo.ToString())
                    .FirstOrDefault();

                Assert.NotNull(resumen);
                Assert.Equal(e.CantidadDeAnimales, resumen.Cantidad);

                var log = context.ReadModeling
                    .Where(x => x.CorrelationId == correlationId)
                    .FirstOrDefault();
                Assert.NotNull(log);
            }
        }
    }
}
