
namespace Journey.Worker.Portal
{
    public interface IPortalWorkCoordinator
    {
        bool WorkerIsWorking { get; }

        bool IsRebuilding { get; }

        void SetWorkerIsNowWorking();

        void SetWorkerIsNotWorking();

        void SetPortalIsNowRebuilding();

        void SetPortalStoppedRebuilding();

        object LockObject { get; }
    }
}
