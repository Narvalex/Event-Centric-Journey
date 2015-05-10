using Journey.EventSourcing.EventStoreRebuilding;
namespace Journey.Worker.Rebuilding
{
    public interface IEventStoreRebuilder
    {
        void Rebuild();

        IEventStoreRebuilderPerfCounter PerformanceCounter { get; }
    }
}
