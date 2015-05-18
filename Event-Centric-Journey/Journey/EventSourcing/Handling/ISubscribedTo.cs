using Journey.Messaging;

namespace Journey.EventSourcing
{
    public interface ISubscribedTo { }

    public interface ISubscribedTo<T> : ISubscribedTo
        where T : IEvent
    {
        void Process(T e);
    }
}
