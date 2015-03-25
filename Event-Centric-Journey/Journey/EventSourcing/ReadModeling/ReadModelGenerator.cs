using Journey.Worker;
using System;

namespace Journey.EventSourcing.ReadModeling
{
    public class ReadModelGenerator<C> where C : ReadModelDbContext
    {
        protected readonly Func<C> contextFactory;
        protected readonly IWorkerRoleTracer tracer;

        public ReadModelGenerator(Func<C> contextFactory, IWorkerRoleTracer tracer)
        {
            this.contextFactory = contextFactory;
            this.tracer = tracer;
        }

        /// <summary>
        /// Materialize an event from the bus.
        /// </summary>
        /// <remarks>
        /// The correct approach is to instantiate fresh DataContext objects as required, keeping DataContext instances fairly short-lived. 
        /// The same applies with Entity Framework.
        /// More information: http://www.albahari.com/nutshell/10linqmyths.aspx
        /// </remarks>
        public void Project<E>(ITraceableVersionedEvent @event, Action<C> materialize, string errorMessage = "An error has ocurred while materializing and event") 
            where E : TraceableEventSourcedEntity
        {
            using (var context = this.contextFactory.Invoke())
            {
                if (context.ReadModelIsUpToDate<E, ITraceableVersionedEvent>(@event))
                {
                    tracer.Notify(errorMessage);
                    return;
                }

                materialize(context);
            }
        }
    }

    public class ReadModelGenerator
    {
        protected bool isRebuilding = false;
    }
}
