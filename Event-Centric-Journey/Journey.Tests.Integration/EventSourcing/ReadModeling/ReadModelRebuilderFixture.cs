using Journey.EventSourcing.ReadModeling.Implementation;
using Journey.Worker;
using System;
using Xunit;

namespace Journey.Tests.Integration.EventSourcing.ReadModeling.ReadModelRebuilderFixture
{
    public class GIVEN_event_store_and_read_model : IDisposable
    {


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class GIVEN_events_and_read_model_with_data : GIVEN_event_store_and_read_model
    {

    }

    public class GIVEN_read_model_rebuilder_and_running_worker : GIVEN_events_and_read_model_with_data
    {
        protected ReadModelRebuilder sut;
        protected FakeWorker worker;

        public GIVEN_read_model_rebuilder_and_running_worker()
        {
            this.worker = new FakeWorker();
            this.sut = new ReadModelRebuilder(worker);
        }

        [Fact]
        public void WHEN_rebuilding_THEN_process_all_events_in_memory_AND_commits_to_database_wiping_tables_first()
        {
            Assert.True(this.worker.IsRunning);
            // In Memory proccess all tables
            // In a transaction: truncates all tables and then commits.
            this.sut.Rebuild();
        }
    }

    // FAKES

    public class FakeWorker : IWorkerRole
    {
        public FakeWorker()
        {
            this.IsRunning = true;
        }

        public bool IsRunning { get; set; }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            this.IsRunning = false;
        }

        public IWorkerRoleTracer Tracer
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
