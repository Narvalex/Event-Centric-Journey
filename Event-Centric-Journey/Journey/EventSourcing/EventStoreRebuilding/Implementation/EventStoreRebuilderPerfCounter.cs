using Journey.EventSourcing.RebuildPerfCounting;
using Journey.Utils.SystemTime;
using Journey.Worker;
using System.Collections.Generic;

namespace Journey.EventSourcing.EventStoreRebuilding
{
    public class EventStoreRebuilderPerfCounter : RebuilderPerfCounter, IRebuilderPerfCounter
    {
        public EventStoreRebuilderPerfCounter(ITracer tracer, ISystemTime time)
            : base(tracer, time)
        { }

        public new void OnStartingRebuildProcess(int messageCount)
        {
            this.tracer.Notify("===> STARTING EVENT STORE REBUILDING...");
            base.OnStartingRebuildProcess(messageCount);
        }

        public new void OnOpeningDbConnectionAndCleaning()
        {
            this.tracer.Notify("===> Opening Event Store connection...");
            base.OnOpeningDbConnectionAndCleaning();
        }

        public new void OnDbConnectionOpenedAndCleansed()
        {
            this.tracer.Notify(string.Format("===> Event Store Connection opened!"));
            base.OnDbConnectionOpenedAndCleansed();
        }

        public new void OnStartingStreamProcessing()
        {
            this.tracer.Notify("===> Starting message stream processing...");
            base.OnStartingStreamProcessing();
        }

        public new void OnStreamProcessingFinished()
        {
            this.tracer.Notify("===> Message stream processing Finished!");
            base.OnStreamProcessingFinished();
        }

        public new void OnStartingCommitting()
        {
            base.OnStartingCommitting();
            this.tracer.Notify("===> Starting committing...");
        }

        public new void OnCommitted(int rowsAffected)
        {
            this.tracer.Notify("===> Event Store Rebuild Commited!");
            base.OnCommitted(rowsAffected);
        }

        public new void ShowResults()
        {
            base.ShowResults();


            this.tracer.Notify(new List<string>
            {
                "=======================================================",
                "           EVENT STORE REBUILDING PERFORMANCE          ",
                "=======================================================",
                string.Format(
                "Messages count:                            {0}", base.messageCount),
                string.Format(
                "Rows affected count:                       {0}", base.rowsAffected),
                string.Format(
                "Complex Message Processor speed:           {0}", base.messageProcessingSpeed),
                string.Format(
                "Database commit speed:                     {0}", base.dbCommitSpeed),
                string.Format(
                "Message Log opening and cleaning delay:    {0}", this.openingConnectionDelay.ToString(elapsedTimeFormat)),
                string.Format(
                "Message stream processing delay:           {0}", this.streamProcessingDelay.ToString(elapsedTimeFormat)),
                string.Format(
                "Total rebuild time:                        {0}", base.processDelay.ToString(elapsedTimeFormat))
            }
            .ToArray());
        }
    }
}
