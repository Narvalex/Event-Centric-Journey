
namespace Journey.Worker
{
    public interface IWorkerRolePortal
    {
        void StartWorking();

        void StopWorking();

        void RebuildReadModel();

        void RebuildEventStore();

        bool IsWorking { get; }

        IWorkerRole WorkerRole { get; }
    }
}
