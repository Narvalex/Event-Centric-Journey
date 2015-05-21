using Journey.EventSourcing;

namespace SimpleInventario.Reporting.Events
{
    /// <summary>
    /// No se deberia crear uno que diga reporting. Quizas esto se utilice como logica para otros procesos y 
    /// no quede simplemente como un modulo netamente de reportes.
    /// </summary>
    public class SeActualizoResumenDeAnimalesPorPeriodo : InternalVersionedEvent
    {
        public int Periodo { get; set; }
        public int CantidadDeAnimales { get; set; }
    }
}
