using System.Collections.Generic;

namespace Journey.Messaging
{
    /// <summary>
    /// Abstracts a command bus for clients that sends serialized object payloads through a <see cref="IMessageSender"/> 
    /// and logs the messages in the <see cref="MessageLog"/>.
    /// </summary>
    public interface IClientCommandBus
    {
        /// <summary>
        /// Sends the specified command.
        /// </summary>
        /// <param name="command">The command to ben sent.</param>
        void Send(Envelope<ICommand> command);

        /// <summary>
        /// Sends the specified commands.
        /// </summary>
        /// <param name="commands">The commands to be sent.</param>
        void Send(IEnumerable<Envelope<ICommand>> commands);
    }
}
