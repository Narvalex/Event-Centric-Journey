namespace Journey.Worker.Config
{
    public interface IEventStoreRebuilderConfig
    {
        string NewMessageLogConnectionString { get; }
        string SourceMessageLogConnectionString { get; }
    }
}
