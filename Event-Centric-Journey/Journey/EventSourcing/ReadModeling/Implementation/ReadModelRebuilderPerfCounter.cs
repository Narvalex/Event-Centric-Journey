using Journey.EventSourcing.RebuildPerfCounting;
using Journey.Utils.SystemTime;
using Journey.Worker;
using System.Collections.Generic;
namespace Journey.EventSourcing.ReadModeling
{
    public class ReadModelRebuilderPerfCounter : RebuilderPerfCounter, IRebuilderPerfCounter
    {
        public ReadModelRebuilderPerfCounter(ITracer tracer, ISystemTime time)
            : base(tracer, time)
        { }

        public new void OnStartingRebuildProcess(int messageCount)
        {
            this.tracer.Notify("===> STARTING READ MODEL REBUILDING...");
            base.OnStartingRebuildProcess(messageCount);
        }

        public new void OnOpeningDbConnectionAndCleaning()
        {
            this.tracer.Notify("===> Opening Event Store connection and deleting all read model data...");
            base.OnOpeningDbConnectionAndCleaning();
        }


        public new void OnDbConnectionOpenedAndCleansed()
        {
            this.tracer.Notify(string.Format("===> Event Store Connection is opean. All data of read model are now deleted."));
            base.OnDbConnectionOpenedAndCleansed();
        }

        public new void OnStartingStreamProcessing()
        {
            this.tracer.Notify("===> Starting event processing...");
            base.OnStartingStreamProcessing();
        }

        public new void OnStreamProcessingFinished()
        {
            this.tracer.Notify("===> Event Stream Processing Finished!");
            base.OnStreamProcessingFinished();
        }

        public new void OnStartingCommitting()
        {
            base.OnStartingCommitting();
            this.tracer.Notify("===> Starting committing...");
        }

        public new void ShowResults()
        {
            base.ShowResults();

            this.tracer.Notify(new List<string>
            {
                "=======================================================",
                "           READ MODEL REBUILDING PERFORMANCE           ",
                "=======================================================",
                string.Format(
                "Events count:                          {0}", base.messageCount),
                string.Format(
                "Rows affected count:                   {0}", base.rowsAffected),
                string.Format(
                "Complex Event Processor speed:         {0}", base.messageProcessingSpeed),
                string.Format(
                "Database commit speed:                 {0}", base.dbCommitSpeed),
                string.Format(
                "Opening and cleaning delay:            {0}", this.openingConnectionDelay.ToString(elapsedTimeFormat)),
                string.Format(
                "Event processing delay:                {0}", this.streamProcessingDelay.ToString(elapsedTimeFormat)),
                string.Format(
                "Total rebuild time:                    {0}", base.processDelay.ToString(elapsedTimeFormat))
            }
            .ToArray());
        }
    }
}