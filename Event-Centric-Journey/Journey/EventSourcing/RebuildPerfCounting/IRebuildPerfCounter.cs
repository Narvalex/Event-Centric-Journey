
namespace Journey.EventSourcing.RebuildPerfCounting
{
    public interface IRebuildPerfCounter
    {
        void OnStartingRebuildProcess();

        void ShowResults();
    }
}
