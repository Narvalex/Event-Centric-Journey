using System.Collections.Generic;
using System.Data.Entity;

namespace Journey.Messaging
{
    /// <summary>
    /// Abstracts an event bus that sends serialied object payloads.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes the specified event.
        /// </summary>
        /// <param name="event">The event to be published.</param>
        void Publish(Envelope<IEvent> @event);

        /// <summary>
        /// Publishes the specified events.
        /// </summary>
        /// <param name="events">The events to be published.</param>
        void Publish(IEnumerable<Envelope<IEvent>> events);

        /// <summary>
        /// Reliably publishes the specified events.
        /// </summary>
        void Publish(IEnumerable<Envelope<IEvent>> events, DbContext context);
    }
}
