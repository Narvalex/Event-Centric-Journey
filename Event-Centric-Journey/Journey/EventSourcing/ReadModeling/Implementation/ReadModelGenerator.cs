using Journey.Worker;
using System;
using System.Linq;

namespace Journey.EventSourcing.ReadModeling
{
    public class ReadModelGenerator<T> : IReadModelGenerator<T> where T : ReadModelDbContext
    {
        protected readonly Func<ReadModelDbContext> contextFactory;
        protected readonly IWorkerRoleTracer tracer;

        public ReadModelGenerator(Func<ReadModelDbContext> contextFactory, IWorkerRoleTracer tracer)
        {
            this.contextFactory = contextFactory;
            this.tracer = tracer;
        }

        public void Project(IVersionedEvent e, Action<T> unitOfWork, bool isLiveProjection = true) 
        {
            using (var context = this.contextFactory.Invoke())
            {
                if (isLiveProjection)
                {
                    // If read model is up to date, then no-op.
                    if (context
                        .ProcessedEvents
                        .Where(log =>
                            log.AggregateId == e.SourceId &&
                            log.AggregateType == e.AggregateType &&
                            log.Version >= e.Version)
                        .Any())
                    {

                        tracer.Notify("Read model is up to date for event type: " + e.GetType().ToString());
                        return;
                    } 
                }

                unitOfWork(context as T);

                // Mark as processed in the the subscription log
                context.ProcessedEvents.Add(
                    new ProcessedEvent
                    {
                        AggregateId = e.SourceId,
                        AggregateType = e.AggregateType,
                        Version = e.Version,
                        EventType = e.GetType().Name,
                        CorrelationId = e.CorrelationId
                    });

                if (isLiveProjection)
                    context.SaveChanges();
            }
        }
    }
}
