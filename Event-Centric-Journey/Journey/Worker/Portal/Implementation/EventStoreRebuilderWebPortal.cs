using Journey.EventSourcing.EventStoreRebuilding;
using Journey.Worker.Rebuilding;
using System;
using System.Web.Hosting;

namespace Journey.Worker.Portal
{
    public sealed class EventStoreRebuilderWebPortal : IRegisteredObject
    {
        private static volatile EventStoreRebuilderWebPortal instance;
        private static volatile IEventStoreRebuilder rebuilder;

        private static IPortalTaskCoordinator coordinator;

        public EventStoreRebuilderWebPortal()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public static EventStoreRebuilderWebPortal Instance
        {
            get { return instance; }
        }

        public static EventStoreRebuilderWebPortal CreateNew(IEventStoreRebuilder rebuilderInstance, IPortalTaskCoordinator coordinator)
        {
            try
            {
                lock (coordinator.LockObject)
                {

                    if (rebuilder != null)
                        throw new InvalidOperationException("Can not create new instance of EventStoreRebuilderWebPortal. You should only start one instance!");

                    if (coordinator.PortalIsRebuilding)
                        throw new InvalidOperationException("Can not create new instance of EventStoreRebuilderWebPortal. Is already rebuilding");

                    instance = new EventStoreRebuilderWebPortal();

                    EventStoreRebuilderWebPortal.coordinator = coordinator;
                    EventStoreRebuilderWebPortal.coordinator.SetPortalIsNotRebuilding();
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

        public IEventStoreRebuilderPerfCounter Rebuild()
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
