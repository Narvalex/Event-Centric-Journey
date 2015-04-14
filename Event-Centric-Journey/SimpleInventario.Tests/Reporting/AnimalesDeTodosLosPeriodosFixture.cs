using Journey.Tests.Testing;
using Journey.Utils.Guids;
using SimpleInventario.Events;
using SimpleInventario.Reporting;
using SimpleInventario.Reporting.Events;
using Xunit;

namespace SimpleInventario.Tests.Reporting.AnimalesDeTodosLosPeriodosFixture
{
    public class DADO_reporte_nuevo
    {
        protected IGuidGenerator guid;
        protected EventSourcingTestHelper<AnimalesDeTodosLosPeriodos> sut;

        public DADO_reporte_nuevo()
        {
            this.guid = new SequentialGuid();
            this.sut = new EventSourcingTestHelper<AnimalesDeTodosLosPeriodos>();
            this.sut.Setup(new AnimalesDeTodosLosPeriodosHandler(this.sut.Store));
        }

        [Fact]
        public void CUANDO_se_aggregan_animales_al_inventario_ENTONCES_se_actualiza_el_resumen_del_periodo()
        {
            var idEmpresa = this.guid.NewGuid();
            var animal = this.guid.NewGuid();
            var sucursal = this.guid.NewGuid();

            var hydratingEvent =
                new SeActualizoResumenDeAnimalesPorPeriodo
                {
                    SourceId = idEmpresa,
                    CantidadDeAnimales = 4,
                    Periodo = 2015
                };

            var newEvent =
                new SeAgregaronAnimalesAlInventario
                {
                    SourceId = idEmpresa,
                    IdEmpresa = idEmpresa,
                    Animal = animal,
                    Sucursal = sucursal,
                    Cantidad = 6,
                    Periodo = 2015
                };

            this.sut.Given(hydratingEvent);

            this.sut.When(newEvent);

            var olapEvent = sut.ThenHasOne<SeActualizoResumenDeAnimalesPorPeriodo>();

            Assert.Equal(2015, olapEvent.Periodo);
            Assert.Equal(10, olapEvent.CantidadDeAnimales);
        }
    }
}
