using Journey.EventSourcing.ReadModeling.Implementation;
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

    public class GIVEN_read_model_rebuilder : GIVEN_events_and_read_model_with_data
    {
        protected ReadModelRebuilder sut;

        public GIVEN_read_model_rebuilder()
        {
            this.sut = new ReadModelRebuilder();
        }

        [Fact]
        public void WHEN_rebuilding_THEN_truncates_all_tables_AND_process_all_events_in_memory_AND_commits_to_database()
        {
            this.sut.Rebuild();
        }
    }
}
