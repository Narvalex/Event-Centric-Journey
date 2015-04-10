using Journey.Client;
using Journey.Utils.Guids;
using SimpleInventario.Application.DTOs;
using SimpleInventario.Commands;
using System;

namespace SimpleInventario.Application
{
    public class InventarioApp : IInventarioApp
    {
        private readonly Guid idEmpresa;
        private readonly IClientApplication app;
        private readonly IGuidGenerator guid;

        public InventarioApp(IClientApplication app)
        {
            this.idEmpresa = Guid.Empty;
            this.app = app;
            this.guid = new SequentialGuid();
        }

        public void AgregarAnimales(AgregarAnimalesDto dto)
        {
            this.app.Send(new AgregarAnimales(this.guid.NewGuid(), this.idEmpresa, dto.Animal, dto.Sucursal, dto.Cantidad, dto.Periodo));
        }
    }
}
