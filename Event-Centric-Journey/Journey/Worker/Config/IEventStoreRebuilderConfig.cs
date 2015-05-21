using Journey.Utils.SystemTime;
namespace Journey.Worker.Config
{
    public interface IEventStoreRebuilderConfig
    {
        string SourceEventStoreConnectionString { get; }

        string NewEventStoreConnectionString { get; }

        ISystemTime SystemTime { get; }
    }
}
