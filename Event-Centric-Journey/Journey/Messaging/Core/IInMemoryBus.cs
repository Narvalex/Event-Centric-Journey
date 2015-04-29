using System.Collections.Generic;

namespace Journey.Messaging
{
    public interface IInMemoryBus
    {
        /// <summary>
        /// Sends the specified commands.
        /// </summary>
        /// <param name="commands">The commands to be sent.</param>
        void Send(IEnumerable<ICommand> commands);

        /// <summary>
        /// Publishes the specified events.
        /// </summary>
        /// <param name="events">The events to be published.</param>
        void Publish(IEnumerable<IEvent> events);
    }
}
