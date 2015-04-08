using Journey.Client;
using Journey.Utils.Guids;
using SimpleInventario.Commands;

namespace SimpleInventario.Application
{
    public class InventarioApp : IInventarioApp
    {
        private readonly IApplication app;
        private readonly IGuidGenerator guid;

        public InventarioApp(IApplication app, IGuidGenerator guid)
        {
            this.app = app;
            this.guid = guid;
        }

        public void DefinirNuevoTipoDeArticulo(ArticuloDto articulo)
        {
            var idArticulo = this.guid.NewGuid();
            var command = new DefinirNuevoTipoDeArticulo(idArticulo, idArticulo, articulo.Nombre);
            this.app.Send(command);
        }
    }
}
