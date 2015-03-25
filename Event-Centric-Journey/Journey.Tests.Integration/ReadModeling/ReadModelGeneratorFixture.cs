using Journey.Database;
using Journey.EventSourcing;
using Journey.EventSourcing.ReadModeling;
using Journey.Tests.Integration.ReadModeling.Implementation;
using Journey.Messaging;
using Journey.Serialization;
using Journey.Worker;
using Microsoft.Practices.Unity;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Xunit;

namespace Journey.Tests.Integration.ReadModeling.ReadModelGeneratorFixture
{
    public class GIVEN_event_store : IDisposable
    {
        protected IUnityContainer container;

        protected readonly string dbName = "ReadModelGeneratorFixture";
        protected string connectionString;

        protected SqlCommandWrapper sqlHelper;


        protected ITextSerializer serializer;
        protected Mock<ISnapshotCache> cacheMock = new Mock<ISnapshotCache>();
        protected MessageSender messageSender;
        protected EventBus eventBus;
        protected CommandBus commandBus;

        protected Guid aggregateId;
        protected IEventStore<ItemAggregate> store;

        private IMessagingSettings eventStoreSettings = new MessagingSettings(1, TimeSpan.FromMilliseconds(100));

        public GIVEN_event_store()
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            this.serializer = new JsonTextSerializer();

            var connectionFactory = System.Data.Entity.Database.DefaultConnectionFactory;
            this.connectionString = connectionFactory
                                    .CreateConnection(this.dbName)
                                    .ConnectionString;

            this.messageSender = new MessageSender(connectionFactory, this.dbName, "Bus.Events");

            this.sqlHelper = new SqlCommandWrapper(this.connectionString);

            System.Data.Entity.Database.SetInitializer<EventStoreDbContext>(null);
            using (var context = new EventStoreDbContext(this.connectionString))
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }

            MessagingDbInitializer.CreateDatabaseObjects(this.connectionString, "Bus");

            this.eventBus = new EventBus(this.messageSender, this.serializer);
            this.commandBus = new CommandBus(this.messageSender, this.serializer);

            this.store = new EventStore<ItemAggregate>(
                this.eventBus, 
                this.commandBus, 
                this.serializer, 
                () => (new EventStoreDbContext(this.connectionString)),
                cacheMock.Object,
                new ConsoleWorkerTracer());
        }

        [Fact]
        private void WHEN_saving_aggregate_with_events_THEN_can_rehydrate()
        {
            this.aggregateId = Guid.NewGuid();

            var aggregate = new ItemAggregate(this.aggregateId);
            aggregate.AddItems(1, "silla");
            aggregate.AddItems(2, "mesa");

            this.store.Save(aggregate, new Guid());

            aggregate = this.store.Find(this.aggregateId);

            Assert.NotNull(aggregate);
        }

        public void Dispose()
        {
            this.sqlHelper.DropDatabase();
        }
    }

//    public class GIVEN_view_model_table : GIVEN_event_store, IDisposable
//    {
//        public GIVEN_view_model_table()
//        {
//            this.sqlHelper.ExecuteNonQuery(@"
//  CREATE TABLE [dbo].[ItemsView](
//	[ItemId] [int] NOT NULL,
//	[Name] [nvarchar](50) NULL,
// CONSTRAINT [PK_ItemsView] PRIMARY KEY CLUSTERED 
//(
//	[ItemId] ASC
//)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
//) ON [PRIMARY]
//");
//        }
//    }

    public class GIVEN_read_model_dbContext : GIVEN_event_store, IDisposable
    {
        protected readonly Func<ItemsDbContext> contextFactory;

        public GIVEN_read_model_dbContext()
        {
            System.Data.Entity.Database.SetInitializer<ItemsDbContext>(null);
            this.contextFactory = () => new ItemsDbContext(this.connectionString);
            var context = contextFactory.Invoke();
            var adapter = (IObjectContextAdapter)context;
            var script = adapter.ObjectContext.CreateDatabaseScript();
            context.Database.ExecuteSqlCommand(script);
            context.Dispose();
        }
    }

    //public class GIVEN_generator : GIVEN_view_model_table, IDisposable
    //{
    //    protected ReadModelGenerator sut;

    //    public GIVEN_generator()
    //    {
    //        this.sut = new ReadModelGenerator(new ConnectionStringProvider(base.connectionString));
    //    }
    //}

    //public class GIVEN_generator_and_Orm : GIVEN_read_model_dbContext, IDisposable
    //{
    //    protected ReadModelBuilder sut;

    //    public GIVEN_generator_and_Orm()
    //    {
    //        this.sut = new ReadModelBuilder(new ConnectionStringProvider(base.connectionString));

    //        this.container = this.CreateContainer();
    //    }

    //    private IUnityContainer CreateContainer()
    //    {
    //        var container = new UnityContainer();
    //        container.RegisterType<ItemsDbContext>(new TransientLifetimeManager(), new InjectionConstructor(base.connectionString));
    //        this.sut.Register(container.Resolve<ItemReadModelGenerator>());
    //        return container;
    //    }
    //}

    //public class GIVEN_some_events_in_the_store : GIVEN_generator_and_Orm, IDisposable
    //{
    //    private readonly Guid commandId = Guid.NewGuid();

    //    public GIVEN_some_events_in_the_store()
    //    {
    //        this.aggregateId = Guid.NewGuid();

    //        var aggregate = new ItemAggregate(this.aggregateId);
    //        aggregate.AddItems(1, "silla");
    //        aggregate.AddItems(2, "mesa");
    //        aggregate.AddItems(3, "heladera");
    //        aggregate.AddItems(4, "cocina");

    //        this.store.Save(aggregate, this.commandId);
    //    }

    //    [Fact]
    //    public void GIVEN_events_and_aggregate_types_THEN_can_generate_full_read_model()
    //    {
    //        this.sut.GenerateReadModelFromAllEvents<ItemAggregate, ItemAdded>();
    //    }

    //    [Fact]
    //    public void GIVEN_aggregate_type_THEN_can_generate_full_read_model()
    //    {
    //        this.sut.GenerateReadModelForAggregate<ItemAggregate>();
    //    }

    //    [Fact]
    //    public void DADO_id_del_comando_CUANDO_se_envia_ENTONCES_se_puede_recuperar_la_materializacion_del_evento_eventualmente_consistente()
    //    {
    //        var dao = new ItemsDao(this.contextFactory, 2);

    //        Assert.Throws<TimeoutException>(() => dao.WaitForEventualConsistencyDelay<ItemView>(this.commandId));

    //        this.sut.GenerateReadModelForAggregate<ItemAggregate>();

    //        dao.WaitForEventualConsistencyDelay<ItemView>(this.commandId);
    //    }
    //}

    #region Helpers
    public class ItemsDao : Dao
    {
        public ItemsDao(Func<ReadModelDbContext> contextFactory, int retryPolicy)
            : base(retryPolicy, contextFactory)
        { }
    }


    public class ItemAggregate : EventSourced
    {
        private List<Item> items = new List<Item>();

        public ItemAggregate(Guid id)
            : base(id)
        {
            base.RehydratesFrom<ItemAdded>(this.OnItemAdded);
        }

        public ItemAggregate(Guid id, IEnumerable<ITraceableVersionedEvent> history)
            : this(id)
        {
            base.LoadFrom(history);
        }

        public void AddItems(int id, string name)
        {
            base.Update(new ItemAdded
            {
                ItemId = id,
                Name = name,
            });
        }

        private void OnItemAdded(ItemAdded e)
        {
            this.items.Add(new Item { Id = e.ItemId, Name = e.Name });
        }

        private class Item
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }

    public class ItemAdded : TraceableVersionedEvent
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
    } 
    #endregion
}
