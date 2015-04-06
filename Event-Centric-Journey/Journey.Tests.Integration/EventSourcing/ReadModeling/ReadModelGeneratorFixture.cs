using Journey.Database;
using Journey.EventSourcing;
using Journey.EventSourcing.ReadModeling;
using Journey.Tests.Integration.EventSourcing.ReadModeling;
using Journey.Utils.Guids;
using Journey.Worker;
using System;
using System.Data.Entity;
using System.Linq;
using Xunit;

namespace Journey.Tests.Integration.EventSourcing.ReadModelGeneratorFixture
{
    public class GIVEN_read_model_db_context : IDisposable
    {
        protected string dbName;
        protected string connectionString;

        protected Func<ItemReadModelDbContext> contextFactory;

        public GIVEN_read_model_db_context()
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            this.dbName = "ReadModelGeneratorFixture";
            this.connectionString = System.Data.Entity.Database.DefaultConnectionFactory.CreateConnection(this.dbName).ConnectionString;

            this.contextFactory = () => new ItemReadModelDbContext(this.connectionString);

            using (var context = this.contextFactory.Invoke())
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }
        }

        [Fact]
        public void WHEN_creating_database_and_do_testing_THEN_can_drop_database()
        {
            // Fake Tests.
            Assert.True(true);
        }

        public void Dispose()
        {
            SqlCommandWrapper.DropDatabase(this.connectionString);
        }
    }

    public class GIVEN_generator : GIVEN_read_model_db_context
    {
        protected IReadModelGenerator<ItemReadModelDbContext> sut;

        public GIVEN_generator()
        {
            this.sut = new ReadModelGenerator<ItemReadModelDbContext>(this.contextFactory, new ConsoleWorkerRoleTracer());
        }

        [Fact]
        public void WHEN_receiving_message_once_THEN_proyects()
        {
            var commandId = SequentialGuid.GenerateNewGuid();
            var e = new ItemAdded
            {
                AggregateType = "DomainAggregate",
                CorrelationId = commandId,
                SourceId = Guid.Empty,
                Id = 1,
                Name = "Silla",
                Version = 1
            };

            this.sut.Project(e, (context) =>
            {
                context.Items.Add(new Item { UnidentifiableId = e.Id, Name = e.Name });
            });

            using (var context = this.contextFactory.Invoke())
            {
                var item = context.Items.Where(i => i.UnidentifiableId == 1).FirstOrDefault();
                var log = context.ProcessedEvents.Where(l => l.AggregateId == Guid.Empty && l.Version == 1).FirstOrDefault();

                Assert.Equal("Silla", item.Name);
                Assert.True(log != null);
                Assert.Equal(commandId, log.CorrelationId);
            }
        }

        [Fact]
        public void WHEN_receiving_message_twice_THEN_proyects_only_once()
        {
            var commandId = SequentialGuid.GenerateNewGuid();
            var e = new ItemAdded
            {
                AggregateType = "DomainAggregate",
                CorrelationId = commandId,
                SourceId = Guid.Empty,
                Id = 1,
                Name = "Silla",
                Version = 1
            };

            this.sut.Project(e, (context) =>
            {
                context.Items.Add(new Item { UnidentifiableId = e.Id, Name = e.Name });
            });

            this.sut.Project(e, (context) =>
            {
                context.Items.Add(new Item { UnidentifiableId = e.Id, Name = e.Name });
            });

            using (var context = this.contextFactory.Invoke())
            {
                var item = context.Items.Where(i => i.UnidentifiableId == 1).ToList();
                var log = context.ProcessedEvents.Where(l => l.AggregateId == Guid.Empty && l.Version == 1).ToList();

                Assert.True(item.Count() == 1);
                Assert.True(log.Count() == 1);
            }
        }

        [Fact]
        public void WHEN_receiving_message_twice_concurrently_THEN_proyects_only_once()
        {
            var commandId = SequentialGuid.GenerateNewGuid();
            var e = new ItemAdded
            {
                AggregateType = "DomainAggregate",
                CorrelationId = commandId,
                SourceId = Guid.Empty,
                Id = 1,
                Name = "Silla",
                Version = 1
            };

            try
            {
                this.sut.Project(e, (context) =>
                    {
                        context.Items.Add(new Item { UnidentifiableId = e.Id, Name = e.Name });

                        // Concurrency
                        this.sut.Project(e, (otherContextInstance) =>
                        {
                            otherContextInstance.Items.Add(new Item { UnidentifiableId = e.Id, Name = e.Name });
                        });
                    });

                // Should have thrown an error.
                Assert.True(false);
            }
            catch (Exception)
            {
                using (var context = this.contextFactory.Invoke())
                {
                    var item = context.Items.Where(i => i.UnidentifiableId == 1).ToList();
                    var log = context.ProcessedEvents.Where(l => l.AggregateId == Guid.Empty && l.Version == 1).ToList();

                    Assert.True(item.Count() == 1);
                    Assert.True(log.Count() == 1);
                }
            }


        }
    }

    #region Fakes

    public class ItemAdded : VersionedEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    #endregion
}
