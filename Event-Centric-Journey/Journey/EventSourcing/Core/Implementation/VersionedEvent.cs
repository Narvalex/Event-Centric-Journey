using System;
namespace Journey.EventSourcing
{
    public abstract class VersionedEvent
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
        public string SourceType { get; set; }

        /// <summary>
        /// El identificador del command que inició la tarea. Es últil para rastrear la materialización del aggregate.
        /// Esto se agrega en el motor del event store, antes de impactarlo en la tabla.
        /// </summary>
        public Guid CorrelationId { get; set; }


        public DateTime CreationDate { get; set; }
    }
}
