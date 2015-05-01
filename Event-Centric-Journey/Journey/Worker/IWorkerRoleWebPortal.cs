
namespace Journey.Worker
{
    public interface IWorkerRoleWebPortal
    {
        void StartWorking();

        void StopWorking();

        void RebuildReadModel();

        void RebuildEventStore();

        void RebuildEventStoreAndReadModel();

        bool IsWorking { get; }
    }
}
