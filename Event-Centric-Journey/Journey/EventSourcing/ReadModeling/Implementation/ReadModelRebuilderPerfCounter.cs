using Journey.EventSourcing.RebuildPerfCounting;
using Journey.Utils.SystemTime;
using Journey.Worker;
using System;
using System.Collections.Generic;
namespace Journey.EventSourcing.ReadModeling
{
    public class ReadModelRebuilderPerfCounter : RebuildPerfCounter, IReadModelRebuilderPerfCounter
    {
        private DateTime openConnectionStartTime;
        private TimeSpan openConnectionElapsedTime;

        public ReadModelRebuilderPerfCounter(IWorkerRoleTracer tracer, ISystemTime time)
            : base(tracer, time)
        { }

        protected override void OnStarting()
        {
            this.tracer.Trace("===> STARTING READ MODEL REBUILDING...");
        }

        public void OnOpeningEventStoreConnection()
        {
            this.openConnectionStartTime = time.Now;
            this.tracer.Trace("===> Opening Event Store connection...");
        }


        public void OnEventStoreConnectionOpened()
        {
            this.openConnectionElapsedTime = time.Now - openConnectionStartTime;
            this.tracer.Trace(string.Format("===> Event Store Connection opened. Time elapsed: {0} seconds", this.openConnectionElapsedTime.TotalSeconds.ToString()));
        }

        protected override void OnShowingResults()
        {
            this.tracer.Notify(new List<string>
            {
                "=== READ MODEL REBUILDING RESULTS ===",
                string.Format("Event Store opening connection delay: {0} seconds", this.openConnectionElapsedTime.TotalSeconds.ToString())
            }.ToArray());
        }
    }
}