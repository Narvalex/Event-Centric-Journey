using Journey.EventSourcing;
namespace SimpleInventario.Events.Reporting
{
    public class SeActualizoResumenDeAnimalesPorPeriodo : VersionedEvent
    {
        public int Periodo { get; set; }
        public int CantidadDeAnimales { get; set; }
    }
}
