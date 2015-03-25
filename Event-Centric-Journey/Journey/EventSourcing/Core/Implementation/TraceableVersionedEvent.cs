using System;

namespace Infrastructure.CQRS.EventSourcing
{
    /// <summary>
    /// Un evento versionado. Este tipo de evento es extendido cuando 
    /// por un implementador que se utiliza para persitir eventos versionados 
    /// en el Depósito de Eventos (Event Store).
    /// </summary>
    public abstract class TraceableVersionedEvent : ITraceableVersionedEvent
    {
        /// <summary>
        /// El identificador del command que inició la tarea. Es últil para rastrear la materialización del aggregate.
        /// </summary>
        public Guid TaskCommandId { get; set; }

        /// <summary>
        /// El identificador de la fuente. Típicamente el identificador del 
        /// <see cref="EventSourced"/>.
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// El aggregate al que pertenece el evento.
        /// </summary>
        public string AggregateType { get; set; }

        /// <summary>
        /// El número de versión del evento.
        /// </summary>
        public int Version { get; set; }
    }
}
