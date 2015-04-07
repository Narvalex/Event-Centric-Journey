using Journey.Messaging;
using System;

namespace SimpleInventario.Commands
{
    public class DefinirNuevoTipoDeArticulo : Command
    {
        public DefinirNuevoTipoDeArticulo(Guid id, Guid idArticulo, string nombre)
            : base(id)
        {
            this.IdArticulo = idArticulo;
            this.Nombre = nombre;
        }

        public Guid IdArticulo { get; private set; }
        public string Nombre { get; set; }
    }
}
