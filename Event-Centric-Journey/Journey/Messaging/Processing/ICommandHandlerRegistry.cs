namespace Infrastructure.CQRS.Messaging.Processing
{
    public interface ICommandHandlerRegistry
    {
        void Register(ICommandHandler handler);
    }
}
