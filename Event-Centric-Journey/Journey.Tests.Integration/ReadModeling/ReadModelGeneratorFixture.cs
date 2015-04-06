using Journey.Database;
using Journey.EventSourcing;
using Journey.EventSourcing.ReadModeling;
using Journey.Messaging;
using Journey.Serialization;
using Journey.Tests.Integration.ReadModeling.Implementation;
using Journey.Utils.SystemDateTime;
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
                new ConsoleWorkerRoleTracer(), new LocalDateTime());
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

        public ItemAggregate(Guid id, IEnumerable<IVersionedEvent> history)
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

    public class ItemAdded : VersionedEvent
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
    } 
    #endregion
}
