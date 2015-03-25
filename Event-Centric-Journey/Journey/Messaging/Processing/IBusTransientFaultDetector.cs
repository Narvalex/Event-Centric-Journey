namespace Journey.Messaging.Processing
{
    public interface IBusTransientFaultDetector
    {
        bool CommandWasAlreadyProcessed(object payload);
    }
}
