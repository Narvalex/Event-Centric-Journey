using Journey.EventSourcing;

namespace SimpleInventario.Reporting.Events
{
    public class SeActualizoResumenDeAnimalesPorPeriodo : VersionedEvent
    {
        public int Periodo { get; set; }
        public int CantidadDeAnimales { get; set; }
    }
}
