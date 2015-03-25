using Infrastructure.CQRS.Messaging;
using System;

namespace Infrastructure.CQRS.EventSourcing
{
    /// <summary>
    /// Represents an event message that belongs to an ordered event stream.
    /// </summary>
    public interface ITraceableVersionedEvent : IEvent
    {
        // Representa el command id que esta correlacionado con el evento. El comando que se origino desde la interfaz, no desde un
        // saga.
        Guid TaskCommandId { get; set; } 

        /// <summary>
        /// El aggregate al que pertenece el evento.
        /// </summary>
        string AggregateType { get; }

        /// <summary>
        /// Gets the version or order of the event in the stream.
        /// </summary>
        int Version { get; }
    }
}
