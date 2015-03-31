
namespace Journey.Worker
{
    public interface IWorkerRolePortal
    {
        void StartWorking();

        void StopWorking();

        bool IsWorking { get; }

        IWorkerRole WorkerRole { get; }
    }
}
