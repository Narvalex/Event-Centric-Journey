using Journey.Worker;
using Journey.Worker.Rebuilding;
using Journey.Worker.Tracing;
using SimpleInventario.DomainRegistry;
using SimpleInventario.ReadModel;
using SimpleInventario.Worker;
using System;

namespace Journey.Web.App_Start
{
    partial class SystemContainer
    {
        static partial void Start()
        {            
            // Implement Here you Own Domain Components.
            var tracer = new WebWorkerRoleTracer();

            var worker = new WorkerRole(
                new SimpleInventarioWorkerRegistry(), 
                tracer);

            // Implement here your Own Domain Read Builder Components.
            Action rebuildReadModel = () =>
            {
                var rebuilder = new ReadModelRebuilder<SimpleInventarioDbContext>(
                    new SimpleInventarioReadModelRebuilderRegistry(),
                    WorkerRoleWebPortal.Instance.WorkerRole.Tracer);

                // Stop working Worker Role
                WorkerRoleWebPortal.Instance.StopWorking();

                // do rebuild
                ReadModelRebuilderWebPortal<SimpleInventarioDbContext>
                    .CreateNew(rebuilder).Rebuild();

                // Start Working Again
                WorkerRoleWebPortal.Instance.StartWorking();
            };

            WorkerRoleWebPortal
                .CreateNew(worker, rebuildReadModel)
                .StartWorking();
        }
    }
}


