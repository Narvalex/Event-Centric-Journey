using System;

namespace Journey.EventSourcing
{
    /// <summary>
    /// Un evento versionado. Este tipo de evento es extendido cuando 
    /// por un implementador que se utiliza para persitir eventos versionados 
    /// en el Depósito de Eventos (Event Store).
    /// </summary>
    public abstract class VersionedEvent : IVersionedEvent
    {
        /// <summary>
        /// El identificador de la fuente. Típicamente el identificador del 
        /// <see cref="EventSourced"/>.
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// El número de versión del evento.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// El aggregate al que pertenece el evento.
        /// </summary>
        public string AggregateType { get; set; }

        /// <summary>
        /// El identificador del command que inició la tarea. Es últil para rastrear la materialización del aggregate.
        /// Esto se agrega en el motor del event store, antes de impactarlo en la tabla.
        /// </summary>
        public Guid CorrelationId { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
