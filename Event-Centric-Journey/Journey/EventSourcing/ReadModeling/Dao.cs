using System;
using System.Linq;
using System.Threading;

namespace Journey.EventSourcing.ReadModeling
{
    public abstract class Dao : IDao
    {
        protected readonly Func<ReadModelDbContext> readModelContextFactory;
        private readonly int eventualConsistencyCheckRetryPolicy;

        public Dao(int eventualConsistencyCheckRetryPolicy, Func<ReadModelDbContext> contextFactory)
        {
            if (eventualConsistencyCheckRetryPolicy < 1)
                throw new ArgumentOutOfRangeException("retryPolicy");

            this.eventualConsistencyCheckRetryPolicy = eventualConsistencyCheckRetryPolicy;
            this.readModelContextFactory = contextFactory;
        }

        public void WaitEventualConsistencyDelay<T>(Guid commandId)
            where T : TraceableEventSourcedEntity
        {
            var retry = default(int);
            var isConsistent = false;

            while (retry < this.eventualConsistencyCheckRetryPolicy)
            {
                using (var context = this.readModelContextFactory.Invoke())
                isConsistent = context.Set<T>().Where(e => e.TaskCommandId == commandId).Any();
                
                
                if (isConsistent)
                    break;

                ++retry;
                Thread.Sleep(TimeSpan.FromMilliseconds(100 * retry));
            }

            if (isConsistent == false)
                throw new TimeoutException("No se pudo verificar la consistencia eventual. Espere unos minutos más");
        }
    }
}
