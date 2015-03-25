
namespace Infrastructure.CQRS.Messaging.Processing
{
    public interface ICommandProcessor
    {
        void ProcessMessage(object payload);
    }
}
