using Infrastructure.CQRS.EventSourcing;
using Infrastructure.CQRS.Messaging;
using System.Linq;

namespace Infrastructure.CQRS.Utils.Testing
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
