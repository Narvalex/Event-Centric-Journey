using Journey.Messaging;
using System;

namespace Journey.EventSourcing
{
    /// <summary>
    /// Represents an event message that belongs to an ordered event stream.
    /// </summary>
    public interface IVersionedEvent : IEvent
    {
        /// <summary>
        /// Gets the version or order of the event in the stream.
        /// </summary>
        int Version { get; }


        /// <summary>
        /// El aggregate al que pertenece el evento.
        /// </summary>
        string AggregateType { get; }

        /// <summary>
        /// Representa el command id que esta correlacionado con el evento.
        /// </summary>
        Guid CorrelationId { get; set; } 



    }
}
