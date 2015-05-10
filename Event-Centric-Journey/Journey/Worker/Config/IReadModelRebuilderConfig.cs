using Journey.Utils.SystemTime;
namespace Journey.Worker.Config
{
    public interface IReadModelRebuilderConfig
    {
        string EventStoreConnectionString { get; }

        string ReadModelConnectionString { get; }

        ISystemTime SystemTime { get; }
    }
}
