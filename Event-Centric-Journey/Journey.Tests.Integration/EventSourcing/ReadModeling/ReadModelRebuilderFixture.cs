using Journey.Database;
using Journey.EventSourcing;
using Journey.EventSourcing.ReadModeling;
using Journey.Messaging;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils.Guids;
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
        protected readonly IWorkerRoleTracer tracer;
        protected readonly IEventStore<ItemActor> eventStore;
        protected readonly ITextSerializer serializer;


        public GIVEN_event_store_and_read_model()
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            this.tracer = new ConsoleWorkerRoleTracer();
            this.serializer = new IndentedJsonTextSerializer();

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
                new Mock<ISqlBus>().As<IEventBus>().Object, new Mock<ISqlBus>().As<ICommandBus>().Object, this.serializer, this.eventStoreContextFactory, new Mock<IInMemoryRollingSnapshotProvider>().Object, this.tracer, new LocalDateTime());

        }

        public virtual void Dispose()
        {
            SqlCommandWrapper.DropDatabase(this.readModelConnectionString);
            SqlCommandWrapper.DropDatabase(this.eventStoreConnectionString);
        }
    }

    public class GIVEN_events_and_read_model_with_data : GIVEN_event_store_and_read_model
    {
        public GIVEN_events_and_read_model_with_data()
        {
            var id = SequentialGuid.GenerateNewGuid();
            var actor = new ItemActor(id);
            actor.HandleCommands();
            this.eventStore.Save(actor, id);

            using (var context = this.readModelContextFactory.Invoke())
            {
                context.ReadModelingEvents.Add(
                    new ProjectedEvent
                    {
                        AggregateId = Guid.Empty,
                        CorrelationId = Guid.Empty,
                        AggregateType = "TestAggregate",
                        EventType = "TestEvent",
                        Version = 1
                    });

                context.ReadModelingEvents.Add(
                    new ProjectedEvent
                    {
                        AggregateId = Guid.Empty,
                        CorrelationId = Guid.Empty,
                        AggregateType = "TestAggregate",
                        EventType = "TestEvent",
                        Version = 2
                    });

                context.ReadModelingEvents.Add(
                    new ProjectedEvent
                    {
                        AggregateId = Guid.Empty,
                        CorrelationId = Guid.Empty,
                        AggregateType = "TestAggregate",
                        EventType = "TestEvent",
                        Version = 3
                    });

                context.SaveChanges();
            }
        }
    }

    public class GIVEN_read_model_rebuilder : GIVEN_events_and_read_model_with_data
    {
        protected ReadModelGeneratorEngine<ItemReadModelDbContext> generatorEngine;
        protected ItemReadModelGenerator itemGenerator;
        protected SynchronousEventDispatcher eventDispatcher;
        protected ReadModelRebuilderEngine<ItemReadModelDbContext> sut;
        protected ItemReadModelDbContext rebuildContext; 

        public GIVEN_read_model_rebuilder()
        {
            this.rebuildContext = this.readModelContextFactory.Invoke();
            this.generatorEngine = new ReadModelGeneratorEngine<ItemReadModelDbContext>(rebuildContext, this.tracer);
            this.itemGenerator = new ItemReadModelGenerator(this.generatorEngine);
            this.eventDispatcher = new SynchronousEventDispatcher(this.tracer);
            this.eventDispatcher.Register(itemGenerator);
            this.sut = new ReadModelRebuilderEngine<ItemReadModelDbContext>(this.eventStoreContextFactory, this.serializer, this.eventDispatcher, rebuildContext);
        }

        [Fact]
        public void WHEN_rebuilding_THEN_process_all_events_in_memory_AND_commits_to_database_wiping_tables_first()
        {
            // In Memory proccess all tables
            // In a transaction: delete all tables and then commits. Truncate is not transactional
            this.sut.Rebuild();
        }

        public override void Dispose()
        {
            this.rebuildContext.Dispose();
            base.Dispose();
        }
    }

    // FAKES
}
