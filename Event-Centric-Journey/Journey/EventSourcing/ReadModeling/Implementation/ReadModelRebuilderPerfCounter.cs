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
        private DateTime eventProcessStartTime;

        private TimeSpan openConnectionElapsedTime;
        private TimeSpan eventProcessingElapsedTime;

        public ReadModelRebuilderPerfCounter(ITracer tracer, ISystemTime time)
            : base(tracer, time)
        { }

        protected override void OnStarting()
        {
            this.tracer.Notify("===> STARTING READ MODEL REBUILDING...");
        }

        protected override void OnShowingResults()
        {
            this.tracer.Notify(new List<string>
            {
                "=== READ MODEL REBUILDING RESULTS ===",
                string.Format("Event Store opening connection delay: {0} seconds", this.openConnectionElapsedTime.TotalSeconds.ToString())
            }.ToArray());
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

        public void OnStartingEventProcessing()
        {
            this.eventProcessStartTime = time.Now;
            this.tracer.Notify("===> Starting event processing...");
        }

        public void OnEventStreamProcessingFinished()
        {
            this.eventProcessingElapsedTime = time.Now - this.eventProcessStartTime;
            this.tracer.Notify(string.Format(
                "===> Event processing finished. Time elapsed: {0}",
                this.eventProcessingElapsedTime.ToString("h'h 'm'm 's's'")));
        }
    }
}