using Journey.EventSourcing.EventStoreRebuilding;
using Journey.EventSourcing.RebuildPerfCounting;
namespace Journey.Worker.Rebuilding
{
    public interface IEventStoreRebuilder
    {
        void Rebuild();

        IRebuilderPerfCounter PerformanceCounter { get; }
    }
}
