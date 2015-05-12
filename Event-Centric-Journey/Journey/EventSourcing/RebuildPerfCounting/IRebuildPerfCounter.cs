
namespace Journey.EventSourcing.RebuildPerfCounting
{
    public interface IRebuilderPerfCounter
    {
        void OnStartingRebuildProcess(int messageCount);
        void OnOpeningDbConnectionAndCleaning();
        void OnDbConnectionOpenedAndCleansed();
        void OnStartingStreamProcessing();
        void OnStreamProcessingFinished();
        void OnStartingCommitting();
        void OnCommitted(int rowsAffected);
        void ShowResults();
    }
}
