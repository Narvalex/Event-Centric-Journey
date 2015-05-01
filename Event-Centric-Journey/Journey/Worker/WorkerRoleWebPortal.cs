using Journey.Worker.Rebuilding;
using System;
using System.Web.Hosting;

namespace Journey.Worker
{
    public sealed class WorkerRoleWebPortal : IRegisteredObject, IDisposable
    {
        private static volatile WorkerRoleWebPortal instance;
        private static volatile IWorkerRole worker;
        private static object lockObject = new object();
        private static volatile bool isWorking;

        private static Action rebuildReadModel;
        private static EventStoreRebuilderWebPortal eventStoreRebuilderPortal;

        private WorkerRoleWebPortal()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public static WorkerRoleWebPortal Instance
        {
            get { return instance; }
        }

        public static WorkerRoleWebPortal CreateNew(IWorkerRole workerInstance, Action rebuildReadModel, EventStoreRebuilderWebPortal eventStoreRebuilderPortal)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new WorkerRoleWebPortal();
                        isWorking = false;

                        if (worker != null)
                            throw new InvalidOperationException("You should only start one instance!");

                        worker = workerInstance;
                        WorkerRoleWebPortal.rebuildReadModel = rebuildReadModel;
                        WorkerRoleWebPortal.eventStoreRebuilderPortal = eventStoreRebuilderPortal;
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

        public void RebuildReadModel()
        {
            rebuildReadModel.Invoke();
        }

        public void RebuildEventStore()
        {
            this.StopWorking();
            eventStoreRebuilderPortal.Rebuild();
            this.StartWorking();
        }

        public void RebuildEventStoreAndReadModel()
        {
            this.StopWorking();
            eventStoreRebuilderPortal.Rebuild();
            rebuildReadModel.Invoke();
            this.StartWorking();
        }

        public object LockObject { get { return lockObject; } }

        public void Dispose()
        {
            this.Stop(true);
        }

        public IWorkerRole WorkerRole { get { return worker; } }

        public bool IsWorking { get { return isWorking; } }
    }
}
