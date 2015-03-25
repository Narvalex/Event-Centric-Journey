using Infrastructure.CQRS.Messaging;
using System.Threading.Tasks;

namespace Infrastructure.CQRS.EventSourcing.Handling
{
    public interface IHandlerOf { }

    public interface IHandlerOf<T> : IHandlerOf
        where T : ICommand
    {
        void Handle(T c);
    }

    public interface IHandlerOf<T, S> : IHandlerOf
        where T : ICommand
        where S : IDomainService
    {
        void Handle(T c, S service);
    }

    public interface IHandlerOf<T, S1, S2> : IHandlerOf
        where T : ICommand
        where S1 : IDomainService
        where S2 : IDomainService
    {
        void Handle(T c, S1 service1, S2 service2);
    }

    public interface IAsyncHandlerOf<T, S1, S2> : IHandlerOf
        where T : ICommand
        where S1 : IDomainService
        where S2 : IDomainService
    {
        Task HandleAsync(T c, S1 service1, S2 service2);
    }
}