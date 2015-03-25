namespace Journey.Messaging.Processing
{
    public interface ICommandHandlerRegistry
    {
        void Register(ICommandHandler handler);
    }
}
