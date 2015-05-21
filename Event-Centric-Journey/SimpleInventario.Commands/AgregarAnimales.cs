using Journey.Messaging;
using System;

namespace SimpleInventario.Commands
{
    public class AgregarAnimales : ExternalCommand
    {
        public AgregarAnimales(Guid id,
            Guid idEmpresa,
            Guid animal,
            Guid sucursal,
            int cantidad,
            int periodo)
            : base(id)
        {
            this.IdEmpresa = idEmpresa;
            this.Animal = animal;
            this.Sucursal = sucursal;
            this.Cantidad = cantidad;
            this.Periodo = periodo;
        }

        public Guid IdEmpresa { get; private set; }
        public Guid Animal { get; private set; }
        public Guid Sucursal { get; private set; }
        public int Cantidad { get; private set; }
        public int Periodo { get; private set; }
    }
}
