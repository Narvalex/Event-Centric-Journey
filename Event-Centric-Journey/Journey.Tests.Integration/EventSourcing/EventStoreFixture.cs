using Journey.Database;
using Journey.EventSourcing;
using Journey.Messaging;
using Journey.Serialization;
using Journey.Utils.SystemDateTime;
using Journey.Worker;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Journey.Tests.Integration.EventSourcing
{
    public class EventStoreFixture
    {
        public class GIVEN_a_badly_implemented_aggregate_ctor : IDisposable
        {
            private readonly string connectionString;
            private IEventStore<FakePretendingEventSourcedAggregate> sut;
            private Mock<IEventBus> eventBusMock = new Mock<IEventBus>();
            private Mock<ICommandBus> commandBusMock = new Mock<ICommandBus>();
            private ITextSerializer serializer;
            private Mock<IInMemoryRollingSnapshotProvider> cacheMock = new Mock<IInMemoryRollingSnapshotProvider>();


            public GIVEN_a_badly_implemented_aggregate_ctor()
            {
                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

                this.serializer = CreateSerializer();

                var dbName = typeof(EventStoreFixture).Name;
                
                
                this.connectionString = System.Data.Entity.Database.DefaultConnectionFactory.CreateConnection(dbName).ConnectionString;
                /** FECOPROD **/
                this.connectionString = string.Format("server=(local);Database={0};User Id=sa;pwd =123456", dbName);

                using (var context = new EventStoreDbContext(this.connectionString))
                {
                    if (context.Database.Exists())
                        context.Database.Delete();

                    context.Database.Create();
                }
            }

            [Fact]
            public void WHEN_constructing_with_bad_aggergate_THEN_throws_exception()
            {
                Assert.Throws<InvalidCastException>(() => this.sut =
                    new EventStore<FakePretendingEventSourcedAggregate>(eventBusMock.Object, commandBusMock.Object, this.serializer,
                        () => (new EventStoreDbContext(this.connectionString)), cacheMock.Object, new ConsoleWorkerRoleTracer(), new LocalDateTime()));
            }

            private static ITextSerializer CreateSerializer()
            {
                return new IndentedJsonTextSerializer();
            }

            public void Dispose()
            {
                using (var context = new EventStoreDbContext(this.connectionString))
                {
                    if (context.Database.Exists())
                        context.Database.Delete();
                }
            }
        }

        public class GIVEN_an_aggregate_with_a_badly_implemented_memento : IDisposable
        {
            private readonly string connectionString;
            private IEventStore<FakePretendingEventSourcedMementoAggregate> sut;
            private Mock<IEventBus> eventBusMock = new Mock<IEventBus>();
            private Mock<ICommandBus> commandBusMock = new Mock<ICommandBus>();
            private ITextSerializer serializer;
            private Mock<IInMemoryRollingSnapshotProvider> cacheMock = new Mock<IInMemoryRollingSnapshotProvider>();


            public GIVEN_an_aggregate_with_a_badly_implemented_memento()
            {
                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

                this.serializer = CreateSerializer();

                var dbName = typeof(EventStoreFixture).Name;
                this.connectionString = System.Data.Entity.Database.DefaultConnectionFactory.CreateConnection(dbName).ConnectionString;

                using (var context = new EventStoreDbContext(this.connectionString))
                {
                    if (context.Database.Exists())
                        context.Database.Delete();

                    context.Database.Create();
                }
            }

            [Fact]
            public void WHEN_constructing_with_bad_memento_aggregatge_THEN_throws_exception()
            {
                Assert.Throws<InvalidCastException>(() => this.sut =
                    new EventStore<FakePretendingEventSourcedMementoAggregate>(eventBusMock.Object, commandBusMock.Object, this.serializer,
                        () => (new EventStoreDbContext(this.connectionString)), cacheMock.Object, new ConsoleWorkerRoleTracer(), new LocalDateTime()));
            }

            private static ITextSerializer CreateSerializer()
            {
                return new IndentedJsonTextSerializer();
            }

            public void Dispose()
            {
                using (var context = new EventStoreDbContext(this.connectionString))
                {
                    if (context.Database.Exists())
                        context.Database.Delete();
                }
            }
        }

        public class GIVEN_store_and_bus_with_an_aggregate_in_memory : IDisposable
        {
            internal readonly string dbName;
            internal readonly string connectionString;
            internal IEventStore<FakeItemsAggregate> sut;
            internal ITextSerializer serializer;
            internal Mock<IInMemoryRollingSnapshotProvider> cacheMock = new Mock<IInMemoryRollingSnapshotProvider>();
            internal MessageSender messageSender;
            internal EventBus eventBus;
            internal CommandBus commandBus;
            internal Guid aggregateId;


            public GIVEN_store_and_bus_with_an_aggregate_in_memory()
            {
                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

                this.serializer = CreateSerializer();
                this.dbName = typeof(EventStoreFixture).Name;
                var connectionFactory = System.Data.Entity.Database.DefaultConnectionFactory;
                this.messageSender = new MessageSender(connectionFactory, this.dbName, "Bus.Events");
                this.connectionString = connectionFactory.CreateConnection(this.dbName).ConnectionString;
                
                using (var context = new EventStoreDbContext(this.connectionString))
                {
                    if (context.Database.Exists())
                        context.Database.Delete();

                    context.Database.Create();
                }

                MessagingDbInitializer.CreateDatabaseObjects(this.connectionString, "Bus");



                this.eventBus = new EventBus(this.messageSender, this.serializer);
                this.commandBus = new CommandBus(this.messageSender, this.serializer, new LocalDateTime());

                this.sut =
                    new EventStore<FakeItemsAggregate>(this.eventBus, this.commandBus, this.serializer,
                        () => (new EventStoreDbContext(this.connectionString)), cacheMock.Object, new ConsoleWorkerRoleTracer(), new LocalDateTime());
            }

            [Fact]
            public void WHEN_saving_aggregate_with_just_one_event_THEN_can_rehydrate()
            {
                this.aggregateId = Guid.NewGuid();

                var item = new Item { Id = 1, Name = "item1" };
                var aggregate = new FakeItemsAggregate(aggregateId);
                aggregate.AddItem(item.Id, item.Name, 10);

                this.sut.Save(aggregate, Guid.NewGuid(), new DateTime());

                var retrivedAggregate = this.sut.Find(aggregateId);
                Assert.NotNull(retrivedAggregate);

                Assert.True(retrivedAggregate.itemsQuantity.ContainsKey(item.Id));
                Assert.Equal(retrivedAggregate.itemsQuantity[item.Id], 10);
            }

            [Fact]
            public void WHEN_saving_aggregate_with_a_batch_of_events_THEN_can_rehydrate()
            {
                this.aggregateId = Guid.NewGuid();

                var item = new Item { Id = 1, Name = "item1" };
                var item2 = new Item { Id = 2, Name = "item2" };

                var aggregate = new FakeItemsAggregate(aggregateId);

                aggregate.AddItem(item.Id, item.Name, 10);
                aggregate.AddItem(item2.Id, item2.Name, 10);
                aggregate.AddItem(item.Id, item.Name, 5);

                this.sut.Save(aggregate, Guid.NewGuid(), new DateTime());

                var retrivedAggregate = this.sut.Find(aggregateId);

                Assert.NotNull(retrivedAggregate);
                Assert.True(retrivedAggregate.itemsQuantity.ContainsKey(item.Id));
                Assert.True(retrivedAggregate.itemsQuantity.ContainsKey(item2.Id));
                Assert.Equal(retrivedAggregate.itemsQuantity[item2.Id], 10);
                Assert.Equal(retrivedAggregate.itemsQuantity[item.Id], 15);
            }

            [Fact]
            public void WHEN_retrieving_persisted_aggregate_with_a_batch_of_events_THEN_can_save_it_once_again_with_new_events_and_replay_it_again()
            {
                this.aggregateId = Guid.NewGuid();

                var item = new Item { Id = 1, Name = "item1" };
                var item2 = new Item { Id = 2, Name = "item2" };

                var aggregate = new FakeItemsAggregate(aggregateId);

                aggregate.AddItem(item.Id, item.Name, 10);
                aggregate.AddItem(item2.Id, item2.Name, 10);
                aggregate.AddItem(item.Id, item.Name, 5);

                this.sut.Save(aggregate, Guid.NewGuid(), new DateTime());

                var retrivedAggregate = this.sut.Find(aggregateId);

                Assert.NotNull(retrivedAggregate);
                Assert.True(retrivedAggregate.itemsQuantity.ContainsKey(item.Id));
                Assert.True(retrivedAggregate.itemsQuantity.ContainsKey(item2.Id));
                Assert.Equal(retrivedAggregate.itemsQuantity[item2.Id], 10);
                Assert.Equal(retrivedAggregate.itemsQuantity[item.Id], 15);

                retrivedAggregate.RemoveItem(item2.Id, 7);
                retrivedAggregate.RemoveItem(item.Id, 2);

                this.sut.Save(retrivedAggregate, Guid.NewGuid(), new DateTime());

                var overRetrivedAggregate = this.sut.Find(aggregateId);

                Assert.NotNull(overRetrivedAggregate);
                Assert.True(overRetrivedAggregate.itemsQuantity.ContainsKey(item.Id));
                Assert.True(overRetrivedAggregate.itemsQuantity.ContainsKey(item2.Id));
                Assert.Equal(overRetrivedAggregate.itemsQuantity[item2.Id], 3);
                Assert.Equal(overRetrivedAggregate.itemsQuantity[item.Id], 13);
            }

            private static ITextSerializer CreateSerializer()
            {
                return new IndentedJsonTextSerializer();
            }

            public void Dispose()
            {
                var builder = new SqlConnectionStringBuilder(this.connectionString);
                builder.InitialCatalog = "master";
                builder.AttachDBFilename = string.Empty;

                using (var connection = new SqlConnection(builder.ConnectionString))
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
                                this.dbName);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public class GIVEN_store_and_event_bus_with_a_Saga_in_memory : IDisposable
        {
            internal readonly string dbName;
            internal readonly string connectionString;
            internal IEventStore<FakeItemsSaga> sut;
            internal ITextSerializer serializer;
            internal Mock<IInMemoryRollingSnapshotProvider> cacheMock = new Mock<IInMemoryRollingSnapshotProvider>();
            internal MessageSender commandSender;
            internal MessageSender eventSender;
            internal EventBus eventBus;
            internal CommandBus commandBus;
            internal Guid sagaId;


            public GIVEN_store_and_event_bus_with_a_Saga_in_memory()
            {
                DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

                this.serializer = CreateSerializer();
                this.dbName = typeof(EventStoreFixture).Name;
                var connectionFactory = System.Data.Entity.Database.DefaultConnectionFactory;
                this.commandSender = new MessageSender(connectionFactory, this.dbName, "Bus.Commands");
                this.eventSender = new MessageSender(connectionFactory, this.dbName, "Bus.Events");
                this.connectionString = connectionFactory.CreateConnection(this.dbName).ConnectionString;

                /** FECOPROD **/
                this.connectionString = string.Format("server=(local);Database={0};User Id=sa;pwd =123456", dbName);

                using (var context = new EventStoreDbContext(this.connectionString))
                {
                    if (context.Database.Exists())
                        context.Database.Delete();

                    context.Database.Create();
                }

                MessagingDbInitializer.CreateDatabaseObjects(this.connectionString, "Bus");



                this.eventBus = new EventBus(this.eventSender, this.serializer);
                this.commandBus = new CommandBus(this.commandSender, this.serializer, new LocalDateTime());


                this.sut =
                    new EventStore<FakeItemsSaga>(this.eventBus, this.commandBus, this.serializer,
                        () => (new EventStoreDbContext(this.connectionString)), cacheMock.Object, new ConsoleWorkerRoleTracer(), new LocalDateTime());
            }

            [Fact]
            public void WHEN_saving_saga_with_just_one_event_THEN_can_rehydrate()
            {
                this.sagaId = Guid.NewGuid();

                var item = new Item { Id = 1, Name = "item1" };
                var saga = new FakeItemsSaga(sagaId);
                saga.AddItem(item.Id, item.Name, 10);

                this.sut.Save(saga, Guid.NewGuid(), new DateTime());

                var retrivedAggregate = this.sut.Find(sagaId);
                Assert.NotNull(retrivedAggregate);

                Assert.True(retrivedAggregate.itemsQuantity.ContainsKey(item.Id));
                Assert.Equal(retrivedAggregate.itemsQuantity[item.Id], 10);
            }

            [Fact]
            public void WHEN_saving_saga_with_a_batch_of_events_THEN_can_rehydrate()
            {
                this.sagaId = Guid.NewGuid();

                var item = new Item { Id = 1, Name = "item1" };
                var item2 = new Item { Id = 2, Name = "item2" };

                var saga = new FakeItemsSaga(sagaId);

                saga.AddItem(item.Id, item.Name, 10);
                saga.AddItem(item2.Id, item2.Name, 10);
                saga.AddItem(item.Id, item.Name, 5);

                this.sut.Save(saga, Guid.NewGuid(), new DateTime());

                var retrivedSaga = this.sut.Find(sagaId);

                Assert.NotNull(retrivedSaga);
                Assert.True(retrivedSaga.itemsQuantity.ContainsKey(item.Id));
                Assert.True(retrivedSaga.itemsQuantity.ContainsKey(item2.Id));
                Assert.Equal(retrivedSaga.itemsQuantity[item2.Id], 10);
                Assert.Equal(retrivedSaga.itemsQuantity[item.Id], 15);
            }

            [Fact]
            public void WHEN_rehydrating_saga_with_a_batch_of_events_THEN_can_save_it_once_again_with_new_events_and_replay_it_again()
            {
                this.sagaId = Guid.NewGuid();

                var item = new Item { Id = 1, Name = "item1" };
                var item2 = new Item { Id = 2, Name = "item2" };

                var saga = new FakeItemsSaga(sagaId);

                saga.AddItem(item.Id, item.Name, 10);
                saga.AddItem(item2.Id, item2.Name, 10);
                saga.AddItem(item.Id, item.Name, 5);

                this.sut.Save(saga, Guid.NewGuid(), new DateTime());

                var retrivedSaga = this.sut.Find(sagaId);

                Assert.NotNull(retrivedSaga);
                Assert.True(retrivedSaga.itemsQuantity.ContainsKey(item.Id));
                Assert.True(retrivedSaga.itemsQuantity.ContainsKey(item2.Id));
                Assert.Equal(retrivedSaga.itemsQuantity[item2.Id], 10);
                Assert.Equal(retrivedSaga.itemsQuantity[item.Id], 15);

                retrivedSaga.RemoveItem(item2.Id, 7);
                retrivedSaga.RemoveItem(item.Id, 2);

                this.sut.Save(retrivedSaga, Guid.NewGuid(), new DateTime());

                var overRetrivedSaga = this.sut.Find(sagaId);

                Assert.NotNull(overRetrivedSaga);
                Assert.True(overRetrivedSaga.itemsQuantity.ContainsKey(item.Id));
                Assert.True(overRetrivedSaga.itemsQuantity.ContainsKey(item2.Id));
                Assert.Equal(overRetrivedSaga.itemsQuantity[item2.Id], 3);
                Assert.Equal(overRetrivedSaga.itemsQuantity[item.Id], 13);
            }

            [Fact]
            public void WHEN_saving_saga_with_an_event_and_a_command_THEN_sends_command_to_the_bus_and_can_rehydrate()
            {
                this.sagaId = Guid.NewGuid();

                var item = new Item { Id = 1, Name = "item1" };
                var saga = new FakeItemsSaga(sagaId);
                saga.AddItem(item.Id, item.Name, 10);

                Assert.Empty(saga.Commands);

                this.sut.Save(saga, Guid.NewGuid(), new DateTime());

                var retrivedSaga = this.sut.Find(sagaId);
                Assert.NotNull(retrivedSaga);

                Assert.True(retrivedSaga.itemsQuantity.ContainsKey(item.Id));
                Assert.Equal(retrivedSaga.itemsQuantity[item.Id], 10);

                retrivedSaga.MakeItemReservation(item, 2);
                Assert.NotEmpty(retrivedSaga.Commands);
                Assert.Equal(1, retrivedSaga.Commands.Count());

                /* Command Publishing */
                this.sut.Save(retrivedSaga, Guid.NewGuid(), new DateTime());
                var overRetrivedSaga = this.sut.Find(sagaId);

                Assert.NotNull(overRetrivedSaga);
                Assert.True(retrivedSaga.itemsQuantity.ContainsKey(item.Id));
                Assert.Equal(retrivedSaga.itemsQuantity[item.Id], 8);
            }

            private static ITextSerializer CreateSerializer()
            {
                return new IndentedJsonTextSerializer();
            }

            public void Dispose()
            {
                var builder = new SqlConnectionStringBuilder(this.connectionString);
                builder.InitialCatalog = "master";
                builder.AttachDBFilename = string.Empty;

                using (var connection = new SqlConnection(builder.ConnectionString))
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
                                this.dbName);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public class BuggyEventBus : IEventBus
        {
            public void Publish(Envelope<IEvent> @event)
            {
                throw new InvalidOperationException();
            }

            public void Publish(IEnumerable<Envelope<IEvent>> events)
            {
                throw new InvalidOperationException();
            }


            public void Publish(IEnumerable<Envelope<IEvent>> events, DbContext connection)
            {
                throw new NotImplementedException();
            }
        }

        #region FakeItemsAggregateAndSaga

        public class FakeItemsSaga : Saga
        {
            public Dictionary<int, int> itemsQuantity = new Dictionary<int, int>();

            public FakeItemsSaga(Guid id)
                : base(id)
            {
                //base.RehydratesFrom<ItemAdded>(this.OnItemAdded);
                //base.RehydratesFrom<ItemRemoved>(this.OnItemRemoved);
                //base.RehydratesFrom<ItemReserved>(this.OnItemReserved);
            }

            public FakeItemsSaga(Guid id, IEnumerable<IVersionedEvent> history)
                : this(id)
            {
                this.LoadFrom(history);
            }

            public void AddItem(int id, string name, int quantity)
            {
                base.Update(new ItemAdded { SourceId = Guid.NewGuid(), Id = id, Name = name, Quantity = quantity });
            }

            public void RemoveItem(int id, int quantity)
            {
                base.Update(new ItemRemoved { Id = id, Quantity = quantity });
            }

            public void MakeItemReservation(Item item, int quantity)
            {
                this.AddCommand(new MarkItemAsReserved(item, quantity));
                base.Update(new ItemReserved { Item = item, Quantity = quantity });
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

            public void Rehydrate(ItemRemoved e)
            {
                this.RemoveSeats(e.Id, e.Quantity);
            }

            public void Rehydrate(ItemReserved e)
            {
                this.RemoveSeats(e.Item.Id, e.Quantity);
            }

            private void RemoveSeats(int id, int quantityForRemove)
            {
                var incomingItemInfo = new Item { Id = id };
                var newQuantityValue = quantityForRemove * -1;
                int quantity;
                if (this.itemsQuantity.TryGetValue(incomingItemInfo.Id, out quantity))
                {
                    newQuantityValue += quantity;
                }

                this.itemsQuantity[incomingItemInfo.Id] = newQuantityValue;
            }
        }

        public class FakeItemsAggregate : EventSourced
        {
            public Dictionary<int, int> itemsQuantity = new Dictionary<int, int>();

            public FakeItemsAggregate(Guid id)
                : base(id)
            {
                //base.RehydratesFrom<ItemAdded>(this.OnItemAdded);
                //base.RehydratesFrom<ItemRemoved>(this.OnItemRemoved);
            }

            public FakeItemsAggregate(Guid id, IEnumerable<IVersionedEvent> history)
                : this(id)
            {
                this.LoadFrom(history);
            }

            public void AddItem(int id, string name, int quantity)
            {
                base.Update(new ItemAdded { SourceId = Guid.NewGuid(), Id = id, Name = name, Quantity = quantity });
            }

            public void RemoveItem(int id, int quantity)
            {
                base.Update(new ItemRemoved { SourceId = Guid.NewGuid(), Id = id, Quantity = quantity });
            }

            private void OnItemAdded(ItemAdded e)
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

            private void OnItemRemoved(ItemRemoved e)
            {
                var incomingItemInfo = new Item { Id = e.Id };
                var newQuantityValue = e.Quantity * -1;
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

        public class ItemRemoved : VersionedEvent
        {
            public ItemRemoved()
            { }

            public int Id { get; set; }
            public int Quantity { get; set; }
        }

        public class ItemReserved : VersionedEvent
        {
            public ItemReserved()
            { }

            public Item Item { get; set; }
            public int Quantity { get; set; }
        }

        public class MarkItemAsReserved : Command
        {
            public MarkItemAsReserved(Item item, int quantity)
                : base(new Guid())
            {
                this.Item = item;
                this.Quantity = quantity;
            }

            public Item Item { get; set; }
            public int Quantity { get; set; }
        }

        #endregion

        #region FakePretendingAggregates

        public class FakePretendingEventSourcedAggregate : IEventSourced
        {

            public Guid Id
            {
                get { throw new NotImplementedException(); }
            }

            public int Version
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<IVersionedEvent> Events
            {
                get { throw new NotImplementedException(); }
            }
        }

        public class FakePretendingEventSourcedMementoAggregate : IEventSourced, IMementoOriginator
        {

            public Guid Id
            {
                get { throw new NotImplementedException(); }
            }

            public int Version
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<IVersionedEvent> Events
            {
                get { throw new NotImplementedException(); }
            }

            public IMemento SaveToMemento()
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
