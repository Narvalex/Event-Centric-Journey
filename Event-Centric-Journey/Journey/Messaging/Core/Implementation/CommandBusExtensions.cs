using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.CQRS.Messaging
{
    /// <summary>
    /// Provides usability overloads for <see cref="ICommandBus"/>.
    /// </summary>
    public static class CommandBusExtensions
    {
        public static void Send(this ICommandBus bus, ICommand command)
        {
            bus.Send(new Envelope<ICommand>(command));
        }

        public static void Send(this ICommandBus bus, IEnumerable<ICommand> commands)
        {
            bus.Send(commands.Select(c => new Envelope<ICommand>(c)));
        }
    }
}
