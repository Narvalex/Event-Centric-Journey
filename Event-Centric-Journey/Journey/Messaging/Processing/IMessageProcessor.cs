namespace Infrastructure.CQRS.Messaging.Processing
{
    public interface IMessageProcessor
    {
        void Start();

        void Stop();
    }
}
