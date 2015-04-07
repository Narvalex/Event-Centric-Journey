using Journey.Messaging;

namespace Journey.Client
{
    public interface IApplication
    {
        void SendCommand(ICommand command);
    }
}
