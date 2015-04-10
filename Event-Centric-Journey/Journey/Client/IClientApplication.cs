using Journey.Messaging;

namespace Journey.Client
{
    public interface IClientApplication
    {
        void Send(ICommand command);
    }
}
