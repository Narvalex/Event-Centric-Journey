namespace Journey.Worker.Config
{
    public interface IEventStoreRebuilderConfig
    {
        string EventStoreConnectionString { get; }

        string NewMessageLogConnectionString { get; }

        string SourceMessageLogConnectionString { get; }
    }
}
