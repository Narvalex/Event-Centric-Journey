using Journey.Worker;
using Journey.Worker.Tracing;

namespace Journey.Web.App_Start
{
    partial class UnityConfig
    {
        static partial void Start()
        {            
            // Implement Here you Own Domain Container.
            var worker = new WorkerRole(new DomainComponents(), new WebWorkerRoleTracer());

            WorkerRoleWebPortal.CreateNew(worker).StartWorking();
        }
    }
}


