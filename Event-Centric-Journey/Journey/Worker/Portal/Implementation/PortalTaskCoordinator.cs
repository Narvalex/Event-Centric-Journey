
namespace Journey.Worker.Portal
{
    public class PortalTaskCoordinator : IPortalTaskCoordinator
    {
        private static volatile bool workerIsWorking;
        private static object lockObject = new object();
        private static volatile bool portalIsRebuilding;

        public bool WorkerIsWorking
        {
            get { return workerIsWorking; }
        }

        public bool PortalIsRebuilding
        {
            get { return portalIsRebuilding; }
        }

        public void SetWorkerIsWorking()
        {
            workerIsWorking = true;
        }

        public void SetWorkerIsNotWorking()
        {
            workerIsWorking = false;
        }

        public void SetPortalIsRebuilding()
        {
            portalIsRebuilding = true;
        }

        public void SetPortalIsNotRebuilding()
        {
            portalIsRebuilding = false;
        }

        public object LockObject
        {
            get { return lockObject; }
        }
    }
}
