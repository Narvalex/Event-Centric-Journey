﻿using Journey.Utils.SystemTime;
using Journey.Worker;
using System;

namespace Journey.EventSourcing.RebuildPerfCounting
{
    public abstract class RebuilderPerfCounter
    {
        #region Private Fields
        private bool started;
        protected int messageCount;
        protected int rowsAffected;

        private DateTime openConnectionStartTime;
        private DateTime RebuildProcessStartTime;
        private DateTime streamProcessStartTime;
        private DateTime commitStartTime;

        protected TimeSpan processDelay;
        protected TimeSpan openingConnectionDelay;
        protected TimeSpan streamProcessingDelay;
        protected TimeSpan committingDelay;

        protected double messageProcessingSpeed;
        protected double dbCommitSpeed;

        protected readonly ITracer tracer;
        protected readonly ISystemTime time;

        // time span formatting from: https://msdn.microsoft.com/en-us/library/ee372287%28v=vs.110%29.aspx
        protected const string elapsedTimeFormat = "h' hours 'm' minutes 's\\.fff' seconds'";
        #endregion

        protected RebuilderPerfCounter(ITracer tracer, ISystemTime time)
        {
            this.tracer = tracer;
            this.time = time;
            this.started = false;
        }

        protected void OnStartingRebuildProcess(int messageCount)
        {
            this.started = true;
            this.messageCount = messageCount;
            this.RebuildProcessStartTime = this.time.Now;
        }

        protected void OnOpeningDbConnectionAndCleaning()
        {
            this.openConnectionStartTime = time.Now;
        }

        protected void OnDbConnectionOpenedAndCleansed()
        {
            this.openingConnectionDelay = time.Now - openConnectionStartTime;
        }

        protected void OnStartingStreamProcessing()
        {
            this.streamProcessStartTime = time.Now;
        }

        protected void OnStreamProcessingFinished()
        {
            this.streamProcessingDelay = time.Now - this.streamProcessStartTime;
            this.messageProcessingSpeed = this.messageCount / this.streamProcessingDelay.TotalSeconds;
        }

        protected void OnStartingCommitting()
        {
            this.commitStartTime = time.Now;
        }

        public void OnCommitted(int rowsAffected)
        {
            var now = this.time.Now;
            this.committingDelay = now - this.commitStartTime;
            this.processDelay = now - this.RebuildProcessStartTime;
            this.rowsAffected = rowsAffected;
            this.dbCommitSpeed = this.rowsAffected / this.committingDelay.TotalSeconds;
        }

        protected void ShowResults()
        {
            if (!this.started)
                return;
        }
    }
}
