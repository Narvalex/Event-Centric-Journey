using Journey.Worker;

namespace Journey.Web.App_Start
{
    partial class UnityConfig
    {
        static partial void Start()
        {            
            // Implement Here you Own Domain Container.
            var worker = new WorkerRole(new DomainContainer());

            WorkerRoleWebPortal.CreateNew(worker).StartWorking();
        }
    }
}


