using System.Collections.Generic;
using System.Data.Entity;

namespace Journey.Messaging
{
    /// <summary>
    /// Abstracts the behavior of sending a message.
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        void Send(Message message);

        /// <summary>
        /// Sends a batch of messages.
        /// </summary>
        /// <param name="messages">The messages to be sent.</param>
        void Send(IEnumerable<Message> messages);

        /// <summary>
        /// Sends reliably messages to the bus.
        /// </summary>
        void Send(IEnumerable<Message> messages, DbContext context);
    }
}
