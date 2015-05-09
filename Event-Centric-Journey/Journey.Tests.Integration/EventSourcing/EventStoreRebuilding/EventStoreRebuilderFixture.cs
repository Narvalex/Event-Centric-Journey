using Journey.Database;
using Journey.EventSourcing;
using Journey.EventSourcing.EventStoreRebuilding;
using Journey.EventSourcing.Handling;
using Journey.Messaging;
using Journey.Messaging.Logging;
using Journey.Messaging.Logging.Metadata;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils.SystemTime;
using Journey.Worker;
using Journey.Worker.Config;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using Xunit;

namespace Journey.Tests.Integration.EventSourcing.EventStoreRebuilderFixture
{
    public class GIVEN_eventStoreDb_AND_messageLogDb_AND_logger_AND_rebuilder : IDisposable
    {
        protected readonly IUnityContainer container;

        protected readonly IWorkerRoleTracer tracer;

        protected readonly string eventStoreDbName;
        protected readonly string messageLogDbName;

        protected readonly string eventStoreConnectionString;
        protected readonly string messageLogConnectionString;

        protected readonly IEventStore<FakeItemsSaga> eventStore;
        protected readonly MessageLogHandler logger;
        protected readonly ITextSerializer serializer;

        public GIVEN_eventStoreDb_AND_messageLogDb_AND_logger_AND_rebuilder()
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            this.serializer = new JsonTextSerializer();
            this.tracer = new ConsoleWorkerRoleTracer();


            // Event Store
            this.eventStoreDbName = typeof(EventStoreFixture).Name + "_eventStore";

            // *********************************
            // EN FECOPROD:
            //this.eventStoreConnectionString = string.Format("Data Source=.\\sqlexpress;Initial Catalog={0};Integrated Security=True", this.eventStoreDbName);
            this.eventStoreConnectionString = string.Format("server=(local);Database={0};User Id=sa;pwd =123456", this.eventStoreDbName);
            // BORRAR CUANDO SEA NECESARIO
            //***********************************

            using (var context = new EventStoreDbContext(eventStoreConnectionString))
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }

            // Message Log
            this.messageLogDbName = typeof(EventStoreFixture).Name + "_messageLog";

            // *********************************
            // EN FECOPROD:
            this.messageLogConnectionString = string.Format("server=(local);Database={0};User Id=sa;pwd =123456", this.messageLogDbName);
            //this.messageLogConnectionString = string.Format("Data Source=.\\sqlexpress;Initial Catalog={0};Integrated Security=True", this.messageLogDbName);
            // BORRAR CUANDO SEA NECESARIO
            //***********************************

            using (var context = new MessageLogDbContext(messageLogConnectionString))
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }

            // MessageLogger
            this.logger = new MessageLogHandler(
                new MessageLog(
                    messageLogConnectionString,
                    this.serializer,
                    new StandardMetadataProvider(), new ConsoleWorkerRoleTracer(), new LocalDateTime()));

            this.container = this.CreateContainer(messageLogConnectionString, eventStoreConnectionString);
        }

        [Fact]
        public void GIVEN_messages_WHEN_replaying_THEN_rebuilds_event_store()
        {
            // GIVEN messages 
            var item = new Item { Id = 1, Name = "silla" };
            var aggregateId = Guid.NewGuid();

            var message1 = new AddItem(Guid.NewGuid(), aggregateId, item.Id, item.Name, 2);

            var message2 = new ItemAdded
            {
                Id = item.Id,
                SourceType = typeof(FakeItemsSaga).Name,
                CorrelationId = aggregateId,
                Name = item.Name,
                Quantity = 2,
                SourceId = aggregateId,
                Version = 1
            };

            var message3 = new AddItem(Guid.NewGuid(), aggregateId, item.Id, item.Name, 1);

            var message4 = new ItemAdded
            {
                Id = item.Id,
                SourceType = typeof(FakeItemsSaga).Name,
                CorrelationId = Guid.NewGuid(),
                Name = item.Name,
                Quantity = 1,
                SourceId = aggregateId,
                Version = 2
            };

            // ...logging
            this.logger.Handle(message1);
            this.logger.Handle(message2);
            this.logger.Handle(message3);
            this.logger.Handle(message4);

            // WHEN replaying

            var rebuilder = container.Resolve<IEventStoreRebuilderEngine>();
            rebuilder.Rebuild();

            // THEN rebuilds event store
            /*** Checked by debug info ****/
        }

        #region Helpers
        private IUnityContainer CreateContainer(string logConnectionString, string storeConnectionString)
        {
            var container = new UnityContainer();

            var commandProcessor = new InMemoryCommandProcessor(this.tracer);
            var eventProcessor = new SynchronousEventDispatcher(this.tracer);

            var bus = new InMemoryBus();

            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<ISystemTime>(new LocalDateTime());
            container.RegisterInstance<IWorkerRoleTracer>(this.tracer);
            var snapshoter = new InMemorySnapshotProvider("EventStoreCache", container.Resolve<ISystemTime>());
            container.RegisterInstance<ISnapshotProvider>(snapshoter);

            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());

            container.RegisterType<EventStoreDbContext>(new ContainerControlledLifetimeManager(), new InjectionConstructor(storeConnectionString));
            container.RegisterType<MessageLogDbContext>(new ContainerControlledLifetimeManager(), new InjectionConstructor(logConnectionString));
            container.RegisterType(typeof(IEventStore<>), typeof(InMemoryEventStore<>), new ContainerControlledLifetimeManager());

            container.RegisterInstance<IEventDispatcher>(eventProcessor);
            container.RegisterInstance<ICommandProcessor>(commandProcessor);
            container.RegisterInstance<ICommandHandlerRegistry>(commandProcessor);

            container.RegisterInstance<IInMemoryBus>(bus);

            var config = new FakeConfig(logConnectionString, logConnectionString);
            container.RegisterInstance<IEventStoreRebuilderConfig>(config);

            container.RegisterType<ICommandHandler, FakeItemsSagaHandler>("FakeItemsSagaHandler");

            var commandHandlerRegistry = container.Resolve<ICommandHandlerRegistry>();
            foreach (var commandHandler in container.ResolveAll<ICommandHandler>())
                commandHandlerRegistry.Register(commandHandler);

            eventProcessor.Register(container.Resolve<FakeItemsSagaHandler>());

            container.RegisterType(typeof(IEventStoreRebuilderEngine), typeof(EventStoreRebuilderEngine), new ContainerControlledLifetimeManager());

            return container;
        }

        public class FakeConfig : IEventStoreRebuilderConfig
        {
            public FakeConfig(string source, string newLog)
            {
                this.SourceMessageLogConnectionString = source;
                this.NewMessageLogConnectionString = newLog;
            }

            public string SourceMessageLogConnectionString { get; set; }

            public string NewMessageLogConnectionString { get; set; }

            public string EventStoreConnectionString
            {
                get { throw new NotImplementedException(); }
            }
        }

        #region Fake Domain

        public class FakeItemsSaga : Saga,
            IHandlerOf<AddItem>,
            IRehydratesFrom<ItemAdded>
        {
            public Dictionary<int, int> itemsQuantity = new Dictionary<int, int>();

            public FakeItemsSaga(Guid id)
                : base(id)
            {
            }

            public FakeItemsSaga(Guid id, IEnumerable<IVersionedEvent> history)
                : this(id)
            {
                this.LoadFrom(history);
            }

            public void Handle(AddItem c)
            {
                base.Update(new ItemAdded
                {
                    Id = c.ItemId,
                    Name = c.Name,
                    Quantity = c.Quantity
                });
            }

            public void Rehydrate(ItemAdded e)
            {
                var incomingItemInfo = new Item { Id = e.Id, Name = e.Name };
                var newQuantityValue = e.Quantity;
                int quantity;
                if (this.itemsQuantity.TryGetValue(incomingItemInfo.Id, out quantity))
                {
                    newQuantityValue += quantity;
                }

                this.itemsQuantity[incomingItemInfo.Id] = newQuantityValue;
            }
        }

        public class Item
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ItemAdded : VersionedEvent
        {
            public ItemAdded()
            { }

            public int Id { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
        }

        public class AddItem : Command
        {
            public AddItem(Guid id, Guid itemGuid, int itemId, string name, int quantity)
                : base(id)
            {
                this.ItemGuid = itemGuid;
                this.ItemId = itemId;
                this.Name = name;
                this.Quantity = quantity;
            }

            public Guid ItemGuid { get; set; }
            public int ItemId { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
        }

        public class FakeItemsSagaHandler :
            ICommandHandler<AddItem>,
            IEventHandler<ItemAdded>
        {
            private readonly IEventStore<FakeItemsSaga> store;

            public FakeItemsSagaHandler(IEventStore<FakeItemsSaga> store)
            {
                this.store = store;
            }

            public void Handle(AddItem command)
            {
                var aggregate = this.store.Find(command.ItemGuid);
                if (aggregate == null)
                    aggregate = new FakeItemsSaga(command.ItemGuid);
                aggregate.Handle(command);
                this.store.Save(aggregate, command.Id, new DateTime());
            }

            public void Handle(ItemAdded e)
            {
                Console.WriteLine("Sending email for item {0}", e.Id);
            }
        }

        #endregion

        #region Disposing
        public void Dispose()
        {
            this.DisposeDatabase(this.eventStoreConnectionString, this.eventStoreDbName);
            this.DisposeDatabase(this.messageLogConnectionString, this.messageLogDbName);
        }

        private void DisposeDatabase(string connectionString, string dbName)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = "master";
            builder.AttachDBFilename = string.Empty;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            @"
USE master
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') 
ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
DROP DATABASE [{0}]
",
                            dbName);

                    command.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #endregion
    }
}
