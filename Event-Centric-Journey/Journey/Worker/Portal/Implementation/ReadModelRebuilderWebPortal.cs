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
        private static volatile bool isRebuilding;

        private ReadModelRebuilderWebPortal()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public static ReadModelRebuilderWebPortal<T> Instance
        {
            get { return instance; }
        }

        public static ReadModelRebuilderWebPortal<T> CreateNew(IReadModelRebuilder<T> rebuilderInstance)
        {
            if (instance == null)
            {
                lock (WorkerRoleWebPortal.Instance.LockObject)
                {
                    if (instance == null)
                    {
                        instance = new ReadModelRebuilderWebPortal<T>();
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
            lock (WorkerRoleWebPortal.Instance.LockObject)
            {
                if (isRebuilding)
                    return;

                isRebuilding = true;
            }

            rebuilder.Rebuild();

            lock (WorkerRoleWebPortal.Instance.LockObject)
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
