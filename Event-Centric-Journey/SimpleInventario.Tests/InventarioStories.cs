using Journey.Tests.Testing;
using Journey.Utils.Guids;
using SimpleInventario.Commands;
using SimpleInventario.Events;
using SimpleInventario.Handlers;
using Xunit;

namespace SimpleInventario.Tests.InventarioStories.ComoUsuario
{
    public class DADO_inventario_vacio
    {
        protected IGuidGenerator guid;
        protected EventSourcingTestHelper<Inventario> sut;

        public DADO_inventario_vacio()
        {
            this.guid = new SequentialGuid();
            this.sut = new EventSourcingTestHelper<Inventario>();
            this.sut.Setup(new InventarioHandler(this.sut.Store));
        }

        [Fact(DisplayName = "DADO inventario vacio ENTONCES puedo agregar animales una vez")]
        public void ENTONCES_puedo_agregar_animales_una_vez()
        {
            var command = new AgregarAnimales(this.guid.NewGuid(), this.guid.NewGuid(), this.guid.NewGuid(), this.guid.NewGuid(), 7, 1870);

            this.sut.When(command);

            var e = sut.ThenHasOne<SeAgregaronAnimalesAlInventario>();

            Assert.Equal(command.IdEmpresa, e.IdEmpresa);
            Assert.Equal(command.Animal, e.Animal);
            Assert.Equal(command.Sucursal, e.Sucursal);
            Assert.Equal(command.Cantidad, e.Cantidad);
            Assert.Equal(command.Periodo, e.Periodo);
        }

        [Fact]
        public void CUANDO_agrego_animales_ENTONCES_puedo_seguir_agregando_animales()
        {
            var idEmpresa = this.guid.NewGuid();
            this.sut.Given(
                new SeAgregaronAnimalesAlInventario
                {
                    SourceId = idEmpresa,
                    IdEmpresa = idEmpresa,
                    Animal = this.guid.NewGuid(),
                    Sucursal = this.guid.NewGuid(),
                    Cantidad = 4,
                    Periodo = 2015
                });

            var command = new AgregarAnimales(this.guid.NewGuid(), idEmpresa, this.guid.NewGuid(), this.guid.NewGuid(), 2, 2015);
            this.sut.When(command);

            var e = sut.ThenHasOne<SeAgregaronAnimalesAlInventario>();

            Assert.Equal(command.IdEmpresa, e.IdEmpresa);
            Assert.Equal(command.Animal, e.Animal);
            Assert.Equal(command.Sucursal, e.Sucursal);
            Assert.Equal(command.Cantidad, e.Cantidad);
            Assert.Equal(command.Periodo, e.Periodo);
        }
    }
}
