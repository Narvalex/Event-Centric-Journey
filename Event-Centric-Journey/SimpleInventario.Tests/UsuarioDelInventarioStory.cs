using Journey.Tests.Testing;
using Journey.Utils.Guids;
using SimpleInventario.Commands;
using SimpleInventario.Events;
using SimpleInventario.Handlers;
using System;
using Xunit;

namespace SimpleInventario.Tests.UsuarioDelInventarioStory
{
    public class DADO_inventario_vacio
    {
        private static readonly Guid idArticulo = SequentialGuid.GenerateNewGuid();
        protected EventSourcingTestHelper<Inventario> sut;

        public DADO_inventario_vacio()
        {
            this.sut = new EventSourcingTestHelper<Inventario>();
            this.sut.Setup(new InventarioHandler(this.sut.Store));
        }

        [Fact(DisplayName = "ENTONCES puedo definir un nuevo tipo de articulo")]
        public void ENTONCES_puedo_definir_un_nuevo_tipo_de_articulo()
        {
            var command = new DefinirNuevoTipoDeArticulo(idArticulo, idArticulo, "Silla");

            this.sut.When(command);

            Assert.Equal(idArticulo, sut.ThenHasOne<NuevoTipoDeArticuloDefinido>().IdArticulo);
            Assert.Equal("Silla", sut.ThenHasOne<NuevoTipoDeArticuloDefinido>().Nombre);
        }
    }
}
