using Journey.Worker;
using System;
using System.Linq;
using Journey.Utils;

namespace Journey.EventSourcing.ReadModeling
{
    public class ReadModelGeneratorEngine<T> : IReadModelGeneratorEngine<T> where T : ReadModelDbContext
    {
        private readonly Func<T> liveContextFactory;
        private readonly ReadModelDbContext rebuildContext;
        private readonly IWorkerRoleTracer tracer;
        private readonly bool isLiveProjection;

        /// <summary>
        /// A Live read model generator instance
        /// </summary>
        /// <param name="liveContextFactory">The live context factory</param>
        /// <param name="tracer">The tracer</param>
        public ReadModelGeneratorEngine(Func<T> liveContextFactory, IWorkerRoleTracer tracer)
            : this(tracer)
        {
            this.isLiveProjection = true;
            this.liveContextFactory = liveContextFactory;
        }

        /// <summary>
        /// The rebuild context instance
        /// </summary>
        /// <param name="rebuildContext">The rebuild context instance</param>
        /// <param name="tracer">The tracer</param>
        public ReadModelGeneratorEngine(ReadModelDbContext rebuildContext, IWorkerRoleTracer tracer)
            : this(tracer)
        {
            this.isLiveProjection = false;
            this.rebuildContext = rebuildContext;
        }

        private ReadModelGeneratorEngine(IWorkerRoleTracer tracer)
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
                        .ReadModelingEvents
                        .Where(log =>
                            log.SourceId == e.SourceId &&
                            log.SourceType == e.SourceType &&
                            log.Version >= e.Version)
                        .Any())
                    {

                        tracer.Trace("Read model is up to date for event type: " + e.GetType().ToString());
                        return;
                    }

                    doLiveProjection(context);

                    // Mark as projected in the the subscription log
                    context.ReadModelingEvents.Add(this.BuildProjectedEventEntity(e));

                    context.SaveChanges();
                }
            }
            else
            {
                doRebuildProjection(this.rebuildContext as T);

                this.rebuildContext.AddToUnityOfWork(this.BuildProjectedEventEntity(e));
            }            
        }

        public void Project(IVersionedEvent e, Action<T> doProjectionOrRebuild)
        {
            if (isLiveProjection)
            {
                using (var context = this.liveContextFactory.Invoke())
                {
                    if (context
                        .ReadModelingEvents
                        .Where(log =>
                            log.SourceId == e.SourceId &&
                            log.SourceType == e.SourceType &&
                            log.Version >= e.Version)
                        .Any())
                    {

                        tracer.Trace("Read model is up to date for event type: " + e.GetType().ToString());
                        return;
                    }

                    doProjectionOrRebuild(context);

                    // Mark as projected in the the subscription log
                    context.ReadModelingEvents.Add(this.BuildProjectedEventEntity(e));

                    context.SaveChanges();
                }
            }
            else
            {
                doProjectionOrRebuild(this.rebuildContext as T);

                this.rebuildContext.AddToUnityOfWork(this.BuildProjectedEventEntity(e));
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
                                l.SourceId == e.SourceId &&
                                l.SourceType == e.SourceType &&
                                l.Version >= e.Version)
                            .Any())
                        {
                            tracer.Trace(string.Format("Event {0} was already consumed by {1}", e.GetType().Name, typeof(Log).Name));
                            return;
                        }
                    

                    // Si el proceso esta en vivo, entonces se consume. 
                    // Si se esta reconstruyendo el read model, entonces se 
                    // omite la consumision (por ejemplo: evitar que se envíen 
                    // correos cada vez que se reconstruya el read model.
                    doConsume();

                    // Mark as consumed in the consumers subscription log
                    context.AddToUnityOfWork(this.BuildConsumedEventEntity<Log>(e));

                    context.SaveChanges();
                }

            }
            else
            {
                this.rebuildContext.AddToUnityOfWork(this.BuildConsumedEventEntity<Log>(e));
            }
        }

        private ProjectedEvent BuildProjectedEventEntity(IVersionedEvent e)
        {
            return new ProjectedEvent
            {
                SourceId = e.SourceId,
                SourceType = e.SourceType,
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
                SourceId = e.SourceId,
                SourceType = e.SourceType,
                Version = e.Version,
                EventType = e.GetType().Name,
                CorrelationId = e.CorrelationId
            };
        }
    }
}
