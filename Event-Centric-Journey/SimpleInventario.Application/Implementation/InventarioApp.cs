using Journey.Client;
using Journey.Utils;
using Journey.Utils.Guids;
using Journey.Utils.SystemTime;
using SimpleInventario.Application.DTOs;
using SimpleInventario.Commands;
using System;
using System.Collections.Generic;

namespace SimpleInventario.Application
{
    public class InventarioApp : IInventarioApp
    {
        private readonly Guid idEmpresa;
        private readonly IClientApplication app;
        private readonly IGuidGenerator guid;

        private readonly IDictionary<string, Guid> animales;
        private readonly IDictionary<string, Guid> sucursales;

        public InventarioApp(IClientApplication app)
        {
            this.idEmpresa = Guid.Empty;
            this.app = app;
            this.guid = new SequentialGuid();

            this.animales = this.GetListaDeAnimales();
            this.sucursales = this.GetListaDeSucursales();
        }

        public void AgregarAnimales(AgregarAnimalesDto dto)
        {
            var animal = this.animales.TryGetValue(dto.Animal);
            var sucursal = this.sucursales.TryGetValue(dto.Sucursal);

            this.app.Send(
                new AgregarAnimales(
                    this.guid.NewGuid(),
                    this.idEmpresa,
                    animal,
                    sucursal,
                    dto.Cantidad,
                    dto.Periodo)
                );
        }

        private Dictionary<string, Guid> GetListaDeAnimales()
        {
            var lista = new Dictionary<string, Guid>();

            lista[Animales.Perro] = Guid.Parse("00000000-0000-0000-0000-000000000000");
            lista[Animales.Gato] = Guid.Parse("00000000-0000-0000-0000-000000000001");
            lista[Animales.Tortuga] = Guid.Parse("00000000-0000-0000-0000-000000000002");
            lista[Animales.Vaca] = Guid.Parse("00000000-0000-0000-0000-000000000003");
            lista[Animales.Burro] = Guid.Parse("00000000-0000-0000-0000-000000000004");

            return lista;
        }

        private Dictionary<string, Guid> GetListaDeSucursales()
        {
            var lista = new Dictionary<string, Guid>();

            lista[Sucursales.Asuncion] = Guid.Parse("00000000-0000-0000-0000-000000000005");
            lista[Sucursales.SanLorenzo] = Guid.Parse("00000000-0000-0000-0000-000000000006");
            lista[Sucursales.NewYork] = Guid.Parse("00000000-0000-0000-0000-000000000007");
            lista[Sucursales.Vallemi] = Guid.Parse("00000000-0000-0000-0000-000000000008");

            return lista;
        }
    }

    public static class Animales
    {
        public const string Perro = "Perro";
        public const string Gato = "Gato";
        public const string Tortuga = "Tortuga";
        public const string Vaca = "Vaca";
        public const string Burro = "Burro";
    }

    public static class Sucursales
    {
        public const string Asuncion = "Asunción";
        public const string SanLorenzo = "San Lorenzo";
        public const string NewYork = "New York";
        public const string Vallemi = "Vallemí";
    }
}
