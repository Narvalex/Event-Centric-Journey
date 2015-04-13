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

        public void Project(IVersionedEvent e, Action<T> doProjection, bool isLiveProjection = true) 
        {
            using (var context = this.contextFactory.Invoke())
            {
                if (isLiveProjection)
                {
                    // If read model is up to date, then no-op.
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
                }

                doProjection(context as T);

                // Mark as projected in the the subscription log
                context.ProjectedEvents.Add(
                    new ProjectedEvent
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


        public void Consume<Log>(IVersionedEvent e, Action doConsume, bool isLiveConsuming = true)
            where Log : class, IProcessedEvent, new()
        {
            using (var context = this.contextFactory.Invoke())
            {
                // Si ya se consumio correctamente, entonces no-op
                if (isLiveConsuming)
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
                }

                // Si el proceso esta en vivo, entonces se consume. 
                // Si se esta reconstruyendo el read model, entonces se 
                // omite la consumision (por ejemplo: evitar que se envíen 
                // correos cada vez que se reconstruya el read model.
                if (isLiveConsuming)
                    doConsume();

                // Mark as consumed in the consumers subscription log
                context.AddToUnitOfWork<Log>(
                    new Log
                    {
                        AggregateId = e.SourceId,
                        AggregateType = e.AggregateType,
                        Version = e.Version,
                        EventType = e.GetType().Name,
                        CorrelationId = e.CorrelationId
                    });

                if (isLiveConsuming)
                    context.SaveChanges();
            }
        }
    }
}
