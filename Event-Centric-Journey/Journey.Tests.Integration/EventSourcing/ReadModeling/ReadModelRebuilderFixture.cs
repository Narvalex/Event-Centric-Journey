using Journey.Database;
using Journey.EventSourcing;
using Journey.Messaging;
using Journey.Serialization;
using Journey.Utils.SystemDateTime;
using Journey.Worker;
using Moq;
using System;
using System.Data.Entity;
using Xunit;

namespace Journey.Tests.Integration.EventSourcing.ReadModeling.ReadModelRebuilderFixture
{
    public class GIVEN_event_store_and_read_model : IDisposable
    {
        protected string readModelDbName;
        protected string eventStoreDbName;
        protected string readModelConnectionString;
        protected string eventStoreConnectionString;
        protected Func<ItemReadModelDbContext> readModelContextFactory;
        protected Func<EventStoreDbContext> eventStoreContextFactory;

        protected readonly IEventStore<ItemActor> eventStore;


        public GIVEN_event_store_and_read_model()
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            this.readModelDbName = "ReadModelRebuilderFixture_ReadModel";
            this.eventStoreDbName = "ReadModelRebuilderFixture_EventStore";
            this.readModelConnectionString = System.Data.Entity.Database.DefaultConnectionFactory.CreateConnection(this.readModelDbName).ConnectionString;
            this.eventStoreConnectionString = System.Data.Entity.Database.DefaultConnectionFactory.CreateConnection(this.eventStoreDbName).ConnectionString;

            this.readModelContextFactory = () => new ItemReadModelDbContext(this.readModelConnectionString);
            this.eventStoreContextFactory = () => new EventStoreDbContext(this.eventStoreConnectionString);

            // *********************************
            // EN FECOPROD:

            this.readModelConnectionString = string.Format("server=(local);Database={0};User Id=sa;pwd =123456", this.readModelDbName);
            this.eventStoreConnectionString = string.Format("server=(local);Database={0};User Id=sa;pwd =123456", this.eventStoreDbName);

            // BORRAR CUANDO SEA NECESARIO
            //***********************************


            using (var context = this.readModelContextFactory.Invoke())
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }

            using (var context = this.eventStoreContextFactory.Invoke())
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }

            this.eventStore = new EventStore<ItemActor>(
                new Mock<IEventBus>().Object, new Mock<ICommandBus>().Object, new IndentedJsonTextSerializer(), this.eventStoreContextFactory, new Mock<IInMemoryRollingSnapshotProvider>().Object, new ConsoleWorkerRoleTracer(), new LocalDateTime());

        }

        public void Dispose()
        {
            SqlCommandWrapper.DropDatabase(this.readModelConnectionString);
            SqlCommandWrapper.DropDatabase(this.eventStoreConnectionString);
        }
    }

    public class GIVEN_events_and_read_model_with_data : GIVEN_event_store_and_read_model
    {
        public GIVEN_events_and_read_model_with_data()
        {
            
        }
    }

    public class GIVEN_read_model_rebuilder_and_running_worker : GIVEN_events_and_read_model_with_data
    {
        //protected ReadModelRebuilder sut;

        //public GIVEN_read_model_rebuilder_and_running_worker()
        //{
        //    this.sut = new ReadModelRebuilder();
        //}

        [Fact]
        public void WHEN_rebuilding_THEN_process_all_events_in_memory_AND_commits_to_database_wiping_tables_first()
        {
            // In Memory proccess all tables
            // In a transaction: delete all tables and then commits. Truncate is not transactional
            //this.sut.Rebuild();
        }
    }

    // FAKES
}
