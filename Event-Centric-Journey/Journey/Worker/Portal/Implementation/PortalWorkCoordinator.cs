using System;

namespace Journey.Worker.Portal
{
    public class PortalWorkCoordinator : IPortalWorkCoordinator
    {
        private static volatile bool workerIsWorking;
        private static object lockObject = new object();

        public bool WorkerIsWorking
        {
            get { return workerIsWorking; }
        }

        public bool IsRebuilding()
        {
            throw new NotImplementedException();
        }


        public void SetWorkerIsNowWorking()
        {
            workerIsWorking = true;
        }

        public void SetWorkerIsNotWorking()
        {
            workerIsWorking = false;
        }

        public void SetPortalIsNowRebuilding()
        {
            throw new NotImplementedException();
        }

        public void SetPortalStoppedRebuilding()
        {
            throw new NotImplementedException();
        }

        public object LockObject
        {
            get { return lockObject; }
        }
    }
}
