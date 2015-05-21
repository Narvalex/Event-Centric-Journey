using Journey.EventSourcing;
using System;

namespace SimpleInventario.Events
{
    public class SeAgregaronAnimalesAlInventario : InternalVersionedEvent
    {
        public Guid IdEmpresa { get; set; }
        public Guid Animal { get; set; }
        public Guid Sucursal { get; set; }
        public int Cantidad { get; set; }
        public int Periodo { get; set; }
    }
}
