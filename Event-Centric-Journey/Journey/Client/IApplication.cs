using Journey.Messaging;

namespace Journey.Client
{
    public interface IApplication
    {
        void Send(ICommand command);
    }
}
