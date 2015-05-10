using Journey.Utils.SystemTime;
using Journey.Worker;

namespace Journey.EventSourcing.RebuildPerfCounting
{
    public abstract class RebuildPerfCounter : IRebuildPerfCounter
    {
        private bool started;

        protected readonly IWorkerRoleTracer tracer;
        protected readonly ISystemTime time;


        public RebuildPerfCounter(IWorkerRoleTracer tracer, ISystemTime time)
        {
            this.tracer = tracer;
            this.time = time;
            this.started = false;
        }

        public void OnStartingRebuildProcess()
        {
            this.started = true;

            this.OnStarting();
        }

        public void ShowResults()
        {
            if (!this.started)
                return;

            this.OnShowingResults();
        }

        protected abstract void OnStarting();

        protected abstract void OnShowingResults();
    }
}
