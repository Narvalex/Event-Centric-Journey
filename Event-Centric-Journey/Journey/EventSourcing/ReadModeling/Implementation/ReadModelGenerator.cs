using Journey.Worker;
using System;
using System.Linq;

namespace Journey.EventSourcing.ReadModeling
{
    public class ReadModelGenerator<T> : IReadModelGenerator<T> where T : ReadModelDbContext
    {
        private readonly Func<T> liveContextFactory;
        private readonly ReadModelDbContext rebuildContext;
        private readonly IWorkerRoleTracer tracer;
        private readonly bool isLiveProjection;

        public ReadModelGenerator(Func<T> liveContextFactory, IWorkerRoleTracer tracer)
            : this(tracer)
        {
            this.isLiveProjection = true;
            this.liveContextFactory = liveContextFactory;
        }

        public ReadModelGenerator(ReadModelDbContext context, IWorkerRoleTracer tracer)
            : this(tracer)
        {
            this.isLiveProjection = false;
            this.rebuildContext = context;
        }

        private ReadModelGenerator(IWorkerRoleTracer tracer)
        {
            this.tracer = tracer;
        }



        public void Project(IVersionedEvent e, Action<T> doLiveProjection, Action<T> doRebuildProjection) 
        {
            if (isLiveProjection)
            {
                using (var context = this.liveContextFactory.Invoke())
                {
                    if (context
                        .ProjectedEvents
                        .Where(log =>
                            log.AggregateId == e.SourceId &&
                            log.AggregateType == e.AggregateType &&
                            log.Version >= e.Version)
                        .Any())
                    {

                        tracer.Notify("Read model is up to date for event type: " + e.GetType().ToString());
                        return;
                    }

                    doLiveProjection(context);

                    // Mark as projected in the the subscription log
                    context.ProjectedEvents.Add(this.BuildProjectedEventEntity(e));

                    context.SaveChanges();
                }
            }
            else
            {
                doRebuildProjection(this.rebuildContext as T);

                this.rebuildContext.AddToUnitOfWork<ProjectedEvent>(this.BuildProjectedEventEntity(e));
            }            
        }

        public void Project(IVersionedEvent e, Action<T> doProjectionOrRebuild)
        {
            if (isLiveProjection)
            {
                using (var context = this.liveContextFactory.Invoke())
                {
                    if (context
                        .ProjectedEvents
                        .Where(log =>
                            log.AggregateId == e.SourceId &&
                            log.AggregateType == e.AggregateType &&
                            log.Version >= e.Version)
                        .Any())
                    {

                        tracer.Notify("Read model is up to date for event type: " + e.GetType().ToString());
                        return;
                    }

                    doProjectionOrRebuild(context);

                    // Mark as projected in the the subscription log
                    context.ProjectedEvents.Add(this.BuildProjectedEventEntity(e));

                    context.SaveChanges();
                }
            }
            else
            {
                doProjectionOrRebuild(this.rebuildContext as T);

                this.rebuildContext.AddToUnitOfWork<ProjectedEvent>(this.BuildProjectedEventEntity(e));
            }            
        }


        public void Consume<Log>(IVersionedEvent e, Action doConsume)
            where Log : class, IProcessedEvent, new()
        {
            if (isLiveProjection)
            {
                using (var context = this.liveContextFactory.Invoke())
                {
                        if (context.Set<Log>()
                            .Where(l =>
                                l.AggregateId == e.SourceId &&
                                l.AggregateType == e.AggregateType &&
                                l.Version >= e.Version)
                            .Any())
                        {
                            tracer.Notify(string.Format("Event {0} was already consumed by {1}", e.GetType().Name, typeof(Log).Name));
                            return;
                        }
                    

                    // Si el proceso esta en vivo, entonces se consume. 
                    // Si se esta reconstruyendo el read model, entonces se 
                    // omite la consumision (por ejemplo: evitar que se envíen 
                    // correos cada vez que se reconstruya el read model.
                    doConsume();

                    // Mark as consumed in the consumers subscription log
                    context.AddToUnitOfWork<Log>(this.BuildConsumedEventEntity<Log>(e));

                    context.SaveChanges();
                }

            }
            else
            {
                this.rebuildContext.AddToUnitOfWork<Log>(this.BuildConsumedEventEntity<Log>(e));
            }
        }

        private ProjectedEvent BuildProjectedEventEntity(IVersionedEvent e)
        {
            return new ProjectedEvent
            {
                AggregateId = e.SourceId,
                AggregateType = e.AggregateType,
                Version = e.Version,
                EventType = e.GetType().Name,
                CorrelationId = e.CorrelationId
            };
        }

        private Log BuildConsumedEventEntity<Log>(IVersionedEvent e) 
            where Log : class, IProcessedEvent, new()
        {
            return new Log
            {
                AggregateId = e.SourceId,
                AggregateType = e.AggregateType,
                Version = e.Version,
                EventType = e.GetType().Name,
                CorrelationId = e.CorrelationId
            };
        }
    }
}
