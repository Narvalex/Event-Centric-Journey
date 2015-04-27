namespace Journey.Worker.Config
{
    public interface IReadModelRebuilderConfig
    {
        string EventStoreConnectionString { get; }
    }
}
