using Journey.EventSourcing.ReadModeling;
using Journey.Worker.Rebuilding;
using System;
using System.Web.Hosting;

namespace Journey.Worker.Portal
{
    public sealed class ReadModelRebuilderWebPortal<T> : IRegisteredObject where T : ReadModelDbContext
    {
        private static volatile ReadModelRebuilderWebPortal<T> instance;
        private static volatile IReadModelRebuilder<T> rebuilder;

        private static IPortalTaskCoordinator coordinator;

        private ReadModelRebuilderWebPortal()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public static ReadModelRebuilderWebPortal<T> Instance
        {
            get { return instance; }
        }

        public static ReadModelRebuilderWebPortal<T> CreateNew(IReadModelRebuilder<T> rebuilderInstance, IPortalTaskCoordinator coordinator)
        {
            try
            {
                lock (coordinator.LockObject)
                {
                    if (rebuilder != null)
                        throw new InvalidOperationException("Can not create new instance of ReadModelRebuilderWebPortal<T>. You should only start one instance!");

                    if (coordinator.PortalIsRebuilding)
                        throw new InvalidOperationException("Can not create new instance of ReadModelRebuilderWebPortal<T>. Is already rebuilding");

                    instance = new ReadModelRebuilderWebPortal<T>();



                    ReadModelRebuilderWebPortal<T>.coordinator = coordinator;
                    ReadModelRebuilderWebPortal<T>.coordinator.SetPortalIsNotRebuilding();
                    rebuilder = rebuilderInstance;
                }

                return instance;
            }
            catch (Exception ex)
            {
                WorkerRoleWebPortal.Instance.WorkerRole.Tracer.TraceAsync(ex.Message);
                throw;
            }
        }

        public IReadModelRebuilderPerfCounter Rebuild()
        {
            try
            {
                lock (coordinator.LockObject)
                {
                    if (coordinator.PortalIsRebuilding)
                    {
                        coordinator.SetPortalIsRebuilding();
                        return rebuilder.PerformanceCounter;
                    }
                }

                rebuilder.Rebuild();

                lock (coordinator.LockObject)
                {
                    coordinator.SetPortalIsNotRebuilding();
                }

                return rebuilder.PerformanceCounter;
            }
            catch (Exception ex)
            {
                coordinator.SetPortalIsNotRebuilding();
                WorkerRoleWebPortal.Instance.WorkerRole.Tracer.TraceAsync(ex.Message);
                throw;
            }
            finally
            {
                rebuilder = null;
            }
        }

        public void Stop(bool immediate)
        {
            if (!coordinator.PortalIsRebuilding)
                HostingEnvironment.UnregisterObject(this);
        }
    }
}
