
namespace Journey.Messaging.Processing
{
    public interface ICommandProcessor
    {
        void ProcessMessage(object payload);
    }
}
