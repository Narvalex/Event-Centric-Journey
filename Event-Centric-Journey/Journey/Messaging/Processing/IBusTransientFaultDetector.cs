namespace Journey.Messaging.Processing
{
    public interface IBusTransientFaultDetector
    {
        bool MessageWasAlreadyProcessed(object payload);
    }
}
