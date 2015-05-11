
using Journey.EventSourcing.RebuildPerfCounting;
namespace Journey.EventSourcing.ReadModeling
{
    public interface IReadModelRebuilderPerfCounter : IRebuildPerfCounter
    {
        void OnOpeningEventStoreConnection();

        void OnEventStoreConnectionOpened();

        void OnStartingEventProcessing();

        void OnEventStreamProcessingFinished();
    }
}
