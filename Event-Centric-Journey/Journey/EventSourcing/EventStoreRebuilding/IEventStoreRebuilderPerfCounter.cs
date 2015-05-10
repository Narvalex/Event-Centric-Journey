using Journey.EventSourcing.RebuildPerfCounting;

namespace Journey.EventSourcing.EventStoreRebuilding
{
    public interface IEventStoreRebuilderPerfCounter : IRebuildPerfCounter
    {
        void OnOpeningEventStoreConnection();

        void OnEventStoreConnectionOpened();
    }
}
