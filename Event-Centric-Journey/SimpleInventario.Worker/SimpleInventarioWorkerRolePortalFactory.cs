using Journey.EventSourcing.EventStoreRebuilding;
using Journey.EventSourcing.ReadModeling;
using Journey.EventSourcing.RebuildPerfCounting;
using Journey.Worker;
using Journey.Worker.Portal;
using Journey.Worker.Rebuilding;
using Journey.Worker.Tracing;
using SimpleInventario.ReadModel;
using SimpleInventario.Worker;
using System;

namespace SimpleInventario.DomainRegistry
{
    public class SimpleInventarioWorkerRolePortalFactory
    {
        public static IWorkerRoleWebPortal CreatePortal()
        {
            // Implement Here you Own Domain Components.
            var workerRegistry = new SimpleInventarioWorkerRegistry();

            var tracer = new WebTracer(workerRegistry.Config.SystemTime);

            var coordinator = new PortalTaskCoordinator();

            var worker = new WorkerRole(
                workerRegistry,
                tracer);

            // Implement here your Own Domain Read Builder Components.
            Func<IRebuilderPerfCounter> rebuildReadModel = () =>
            {
                var rebuilder = new ReadModelRebuilder<SimpleInventarioDbContext>(
                    new SimpleInventarioReadModelRebuilderRegistry(),
                    WorkerRoleWebPortal.Instance.WorkerRole.Tracer);

                // do rebuild
                return ReadModelRebuilderWebPortal<SimpleInventarioDbContext>
                    .CreateNew(rebuilder, coordinator)
                    .Rebuild();

            };

            Func<IRebuilderPerfCounter> rebuildEventStore = () =>
            {
                var eventStoreRebuilderPortal = EventStoreRebuilderWebPortal.CreateNew(
                    new EventStoreRebuilder(new SimpleInventarioEventStoreRebuilderRegistry(), tracer),
                    coordinator);

                return eventStoreRebuilderPortal.Rebuild();
            };

            WorkerRoleWebPortal
                .CreateNew(worker, rebuildReadModel, rebuildEventStore, coordinator)
                .StartWorking();

            return WorkerRoleWebPortal.Instance;
        }
    }
}
