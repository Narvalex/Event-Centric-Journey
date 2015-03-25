using Journey.EventSourcing;
using Journey.Messaging;
using System.Linq;

namespace Journey.Utils.Testing
{
    public static class EnumerableCommandsExtensions
    {
        public static TCommand SingleCommand<TCommand>(this ISaga saga)
            where TCommand : ICommand
        {
            return (TCommand)saga.Commands.Single();
        }
    }
}
