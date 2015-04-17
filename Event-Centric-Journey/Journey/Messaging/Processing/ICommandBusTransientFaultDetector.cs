namespace Journey.Messaging.Processing
{
    public interface ICommandBusTransientFaultDetector
    {
        bool CommandWasAlreadyProcessed(object payload);
    }
}
