using Journey.EventSourcing.EventStoreRebuilding;
using Journey.EventSourcing.ReadModeling;
using System;
using System.Web.Hosting;

namespace Journey.Worker.Portal
{
    public sealed class WorkerRoleWebPortal : IWorkerRoleWebPortal, IRegisteredObject, IDisposable
    {
        private static volatile WorkerRoleWebPortal instance;
        private static volatile IWorkerRole worker;

        private static IPortalTaskCoordinator coordinator;

        private static Func<IReadModelRebuilderPerfCounter> rebuildReadModel;
        private static Func<IEventStoreRebuilderPerfCounter> rebuildEventStore;

        private WorkerRoleWebPortal()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public static WorkerRoleWebPortal Instance
        {
            get { return instance; }
        }

        public static WorkerRoleWebPortal CreateNew(IWorkerRole workerInstance, Func<IReadModelRebuilderPerfCounter> rebuildReadModel, Func<IEventStoreRebuilderPerfCounter> rebuildEventStore, IPortalTaskCoordinator coordinator)
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

            //this.StartWorking();
        }

        public void RebuildEventStore()
        {
            this.StopWorking();

            rebuildEventStore
                .Invoke()
                .ShowResults();

            //this.StartWorking();
        }

        public void RebuildEventStoreAndReadModel()
        {
            this.StopWorking();

            var eventStoreRebuildingResults = rebuildEventStore.Invoke();
            var readModelRebuildingResults = rebuildReadModel.Invoke();

            readModelRebuildingResults.ShowResults();
            eventStoreRebuildingResults.ShowResults();

            //this.StartWorking();
        }

        public void Dispose()
        {
            this.Stop(true);
        }

        public IWorkerRole WorkerRole { get { return worker; } }

        public bool IsWorking { get { return coordinator.WorkerIsWorking; } }
    }
}
