namespace Infrastructure.CQRS.Messaging.Processing
{
    public interface IEventHandlerRegistry
    {
        void Register(IEventHandler handler);
    }
}
