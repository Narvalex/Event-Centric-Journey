using System;
using System.Collections.Generic;
using System.Web.Hosting;

namespace Journey.Worker
{
    public sealed class WorkerRoleManager : IRegisteredObject, IDisposable
    {
        private static volatile WorkerRoleManager instance;
        private static volatile IWorker worker;
        private static object lockObject = new object();
        private static volatile bool isWorking;

        private WorkerRoleManager() 
        {
            HostingEnvironment.RegisterObject(this);
        }

        public static WorkerRoleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    return null;
                }

                return instance;
            }
        }

        public static WorkerRoleManager CreateNew(IWorker workerInstance)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new WorkerRoleManager();
                        isWorking = false;

                        if (worker != null)
                            throw new InvalidOperationException("You should only start one instance!");

                        worker = workerInstance;
                    }
                }
            }

            return instance;
        }

        public void StartWorking()
        {
            lock (lockObject)
            {
                if (isWorking)
                    return;

                isWorking = true;
                worker.Start();
            }
        }
        
        public void StopWorking()
        {
            lock (lockObject)
            {
                if (!isWorking)
                    return;

                worker.Stop();
                isWorking = false;
            }
        }

        public void Stop(bool immediate)
        {
            this.StopWorking();
            worker.Dispose();
            HostingEnvironment.UnregisterObject(this);
        }

        public void Dispose()
        {
            this.Stop(true);
        }
    }
}
