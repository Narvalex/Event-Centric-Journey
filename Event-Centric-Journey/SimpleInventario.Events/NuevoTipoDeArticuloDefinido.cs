using Journey.EventSourcing;
using System;

namespace SimpleInventario.Events
{
    public class NuevoTipoDeArticuloDefinido : VersionedEvent
    {
        public Guid IdArticulo { get; set; }
        public string Nombre { get; set; }
    }
}
