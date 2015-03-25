using Infrastructure.CQRS.Messaging;

namespace Infrastructure.CQRS.EventSourcing
{
    public interface ISubscribedTo { }

    public interface ISubscribedTo<T> : ISubscribedTo
        where T : IEvent
    {
        void BeNotifiedOf(T e);
    }
}
