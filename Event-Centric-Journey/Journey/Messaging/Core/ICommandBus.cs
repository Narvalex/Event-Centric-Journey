using System.Collections.Generic;
using System.Data.Entity;

namespace Infrastructure.CQRS.Messaging
{
    /// <summary>
    /// Abstracts a command bus that sends serialized object payloads through a <see cref="IMessageSender"/>.
    /// </summary>
    public interface ICommandBus
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

        /// <summary>
        /// Reliably sends the specified commands.
        /// </summary>
        void Send(IEnumerable<Envelope<ICommand>> commands, DbContext context);
    }
}
