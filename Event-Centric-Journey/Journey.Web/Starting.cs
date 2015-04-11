using Journey.Worker;
using Journey.Worker.Tracing;

namespace Journey.Web.App_Start
{
    partial class UnityConfig
    {
        static partial void Start()
        {            
            // Implement Here you Own Domain Components.
            //var worker = new WorkerRole(new FakeDomainWorker(), new WebWorkerRoleTracer());

            var worker = new WorkerRole(new SimpleInventario.Worker.SimpleInventarioWorkerRegistry(), new WebWorkerRoleTracer());

            WorkerRoleWebPortal.CreateNew(worker).StartWorking();
        }
    }

    /// <summary>
    /// A fake domain worker
    /// </summary>
    public class FakeDomainWorker : DomainWorkerRegistry
    { }
}


