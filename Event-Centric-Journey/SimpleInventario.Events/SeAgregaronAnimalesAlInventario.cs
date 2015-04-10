using Journey.EventSourcing;
using System;

namespace SimpleInventario.Events
{
    public class SeAgregaronAnimalesAlInventario : VersionedEvent
    {
        public Guid IdEmpresa { get; set; }
        public string Animal { get; set; }
        public string Sucursal { get; set; }
        public int Cantidad { get; set; }
        public int Periodo { get; set; }
    }
}
