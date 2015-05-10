using Journey.EventSourcing.RebuildPerfCounting;
using Journey.Utils.SystemTime;
using Journey.Worker;
using System;

namespace Journey.EventSourcing.EventStoreRebuilding
{
    public class EventStoreRebuilderPerfCounter : RebuildPerfCounter, IEventStoreRebuilderPerfCounter
    {
        private DateTime openConnectionStartTime;
        private TimeSpan openConnectionElapsedTime;

        public EventStoreRebuilderPerfCounter(IWorkerRoleTracer tracer, ISystemTime time)
            : base(tracer, time)
        { }

        protected override void OnStarting()
        {
            this.tracer.Notify("===> STARTING EVENT STORE REBUILDING...");
        }

        public void OnOpeningEventStoreConnection()
        {
            this.openConnectionStartTime = time.Now;
            this.tracer.Notify("===> Opening Event Store connection...");
        }

        public void OnEventStoreConnectionOpened()
        {
            this.openConnectionElapsedTime = time.Now - openConnectionStartTime;
            this.tracer.Notify(string.Format("===> Event Store Connection opened. Time elapsed: {0} seconds", this.openConnectionElapsedTime.TotalSeconds.ToString()));
        }

        protected override void OnShowingResults()
        {
            this.tracer.Notify("=== EVENT STORE REBUILDING RESULTS ===");
            this.tracer.Notify(string.Format("Event Store opening connection delay: {0} seconds", this.openConnectionElapsedTime.TotalSeconds.ToString()));
        }
    }
}
