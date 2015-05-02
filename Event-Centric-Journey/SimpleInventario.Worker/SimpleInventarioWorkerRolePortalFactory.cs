﻿using Journey.Worker;
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
            var tracer = new WebWorkerRoleTracer();

            var coordinator = new PortalTaskCoordinator();

            var worker = new WorkerRole(
                new SimpleInventarioWorkerRegistry(),
                tracer);

            // Implement here your Own Domain Read Builder Components.
            Action rebuildReadModel = () =>
            {
                var rebuilder = new ReadModelRebuilder<SimpleInventarioDbContext>(
                    new SimpleInventarioReadModelRebuilderRegistry(),
                    WorkerRoleWebPortal.Instance.WorkerRole.Tracer);

                // do rebuild
                ReadModelRebuilderWebPortal<SimpleInventarioDbContext>
                    .CreateNew(rebuilder, coordinator).Rebuild();
            };

            Action rebuildEventStore = () =>
            {
                var eventStoreRebuilder = EventStoreRebuilderWebPortal.CreateNew(
                    new EventStoreRebuilder(new SimpleInventarioEventStoreRebuilderRegistry(), tracer),
                    coordinator);

                eventStoreRebuilder.Rebuild();
            };

            WorkerRoleWebPortal
                .CreateNew(worker, rebuildReadModel, rebuildEventStore, coordinator)
                .StartWorking();

            return WorkerRoleWebPortal.Instance;
        }
    }
}