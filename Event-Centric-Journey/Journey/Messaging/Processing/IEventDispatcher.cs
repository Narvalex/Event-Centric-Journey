
namespace Journey.Messaging.Processing
{
    public interface IEventDispatcher
    {
        void Register(IEventHandler handler);

        void DispatchMessage(IEvent @event, string messageId, string correlationId, string traceIdentifier);
    }
}
