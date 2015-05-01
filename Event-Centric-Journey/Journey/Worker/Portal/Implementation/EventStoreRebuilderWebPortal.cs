using Journey.Worker.Rebuilding;
using System;
using System.Web.Hosting;

namespace Journey.Worker.Portal
{
    public sealed class EventStoreRebuilderWebPortal : IRegisteredObject
    {
        private static volatile EventStoreRebuilderWebPortal instance;
        private static volatile IEventStoreRebuilder rebuilder;

        private static volatile bool isRebuilding;

        private static IPortalWorkCoordinator coordinator;

        public EventStoreRebuilderWebPortal()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public static EventStoreRebuilderWebPortal Instance
        {
            get { return instance; }
        }

        public static EventStoreRebuilderWebPortal CreateNew(IEventStoreRebuilder rebuilderInstance)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new EventStoreRebuilderWebPortal();
                        isRebuilding = false;

                        if (rebuilder != null)
                            throw new InvalidOperationException("You should only start one instance!");

                        rebuilder = rebuilderInstance;
                    }
                }
            }

            return instance;
        }

        public void Rebuild()
        {
            lock (lockObject)
            {
                if (isRebuilding)
                    return;

                isRebuilding = true;
            }

            rebuilder.Rebuild();

            lock (lockObject)
            {
                isRebuilding = false;
            }
        }

        public void Stop(bool immediate)
        {
            if (!isRebuilding)
                HostingEnvironment.UnregisterObject(this);
        }
    }
}
