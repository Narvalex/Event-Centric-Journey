
namespace Journey.Worker.Portal
{
    public interface IPortalTaskCoordinator
    {
        bool WorkerIsWorking { get; }

        bool PortalIsRebuilding { get; }

        void SetWorkerIsWorking();

        void SetWorkerIsNotWorking();

        void SetPortalIsRebuilding();

        void SetPortalIsNotRebuilding();

        object LockObject { get; }
    }
}
