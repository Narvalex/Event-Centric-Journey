using Journey.EventSourcing.RebuildPerfCounting;
using System;
using System.Web.Hosting;

namespace Journey.Worker.Portal
{
    public sealed class WorkerRoleWebPortal : IWorkerRoleWebPortal, IRegisteredObject, IDisposable
    {
        private static volatile WorkerRoleWebPortal instance;
        private static volatile IWorkerRole worker;

        private static IPortalTaskCoordinator coordinator;

        private static Func<IRebuilderPerfCounter> rebuildReadModel;
        private static Func<IRebuilderPerfCounter> rebuildEventStore;

        private WorkerRoleWebPortal()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public static WorkerRoleWebPortal Instance
        {
            get { return instance; }
        }

        public static WorkerRoleWebPortal CreateNew(IWorkerRole workerInstance, Func<IRebuilderPerfCounter> rebuildReadModel, Func<IRebuilderPerfCounter> rebuildEventStore, IPortalTaskCoordinator coordinator)
        {
            if (instance == null)
            {
                lock (coordinator.LockObject)
                {
                    if (instance == null)
                    {
                        instance = new WorkerRoleWebPortal();


                        if (worker != null)
                            throw new InvalidOperationException("You should only start one instance!");


                        WorkerRoleWebPortal.coordinator = coordinator;
                        WorkerRoleWebPortal.coordinator.SetWorkerIsNotWorking();
                        worker = workerInstance;
                        WorkerRoleWebPortal.rebuildReadModel = rebuildReadModel;
                        WorkerRoleWebPortal.rebuildEventStore = rebuildEventStore;
                    }
                }
            }

            return instance;
        }

        public void StartWorking()
        {
            lock (coordinator.LockObject)
            {
                if (coordinator.WorkerIsWorking)
                    return;

                coordinator.SetWorkerIsWorking();
                worker.Start();
            }
        }

        public void StopWorking()
        {
            lock (coordinator.LockObject)
            {
                if (!coordinator.WorkerIsWorking)
                    return;

                worker.Stop();
                coordinator.SetWorkerIsNotWorking();
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
            this.StopWorking();

            rebuildReadModel
                .Invoke()
                .ShowResults();
        }

        public void RebuildEventStore()
        {
            this.StopWorking();

            rebuildEventStore
                .Invoke()
                .ShowResults();
        }

        public void RebuildEventStoreAndReadModel()
        {
            this.StopWorking();

            var eventStoreRebuildingResults = rebuildEventStore.Invoke();
            var readModelRebuildingResults = rebuildReadModel.Invoke();

            readModelRebuildingResults.ShowResults();
            eventStoreRebuildingResults.ShowResults();
        }

        public void Dispose()
        {
            this.Stop(true);
        }

        public IWorkerRole WorkerRole { get { return worker; } }

        public bool IsWorking { get { return coordinator.WorkerIsWorking; } }


        public ITracer Tracer
        {
            get { return worker.Tracer; }
        }
    }
}
