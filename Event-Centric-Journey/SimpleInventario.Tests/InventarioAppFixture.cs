using Journey.Tests.Testing.Fakes;
using Journey.Utils.Guids;
using SimpleInventario.Application;
using SimpleInventario.Application.DTOs;
using SimpleInventario.Commands;
using System;
using System.Linq;
using Xunit;

namespace SimpleInventario.Tests.InventarioAppFixture
{
    public class DADO_app
    {
        protected readonly FakeApp app;
        protected readonly InventarioApp sut;

        public DADO_app()
        {
            this.app = new FakeApp();
            this.sut = new InventarioApp(app);
        }

        [Fact]
        public void ENTONCES_se_pueden_agregar_animales()
        {
            var dto = new AgregarAnimalesDto
            {
                Animal = "Perro",
                Cantidad = 5,
                Periodo = 2015,
                Sucursal = "Azotey"
            };

            this.sut.AgregarAnimales(dto);

            Assert.Equal(1, this.app.Commands.Count);

            var command = this.app.Commands.FirstOrDefault() as AgregarAnimales;
            Assert.Equal(Guid.Empty, command.IdEmpresa);
            Assert.Equal(dto.Animal, command.Animal);
            Assert.Equal(dto.Cantidad, command.Cantidad);
            Assert.Equal(dto.Periodo, command.Periodo);
            Assert.Equal(dto.Sucursal, command.Sucursal);
        }
    }
}
