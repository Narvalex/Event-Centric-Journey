namespace Journey.Messaging.Processing
{
    public interface IEventHandlerRegistry
    {
        void Register(IEventHandler handler);
    }
}
