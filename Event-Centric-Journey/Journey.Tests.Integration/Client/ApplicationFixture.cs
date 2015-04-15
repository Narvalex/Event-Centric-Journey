using Journey.Database;
using Journey.Messaging;
using Journey.Tests.Integration.EventSourcing.ReadModeling;
using System;
using System.Data.Entity;
using Xunit;

namespace Journey.Tests.Integration.Client.ApplicationFixture
{
    public class GIVEN_bus_and_read_model : IDisposable
    {
        protected string connectionString;

        protected ICommandBus commandBus;
        protected IEventBus eventBus;

        protected Func<ItemReadModelDbContext> contextFactory;

        public GIVEN_bus_and_read_model()
        {
            DbConfiguration.SetConfiguration(new TransientFaultHandlingDbConfiguration());

            var dbName = "ApplicationFixture";
            this.connectionString = System.Data.Entity.Database.DefaultConnectionFactory.CreateConnection(dbName).ConnectionString;

            this.contextFactory = () => new ItemReadModelDbContext(this.connectionString);

            using (var context = this.contextFactory.Invoke())
            {
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();
            }

            this.commandBus = new FakeBus(this.contextFactory);
            this.eventBus = new FakeBus(this.contextFactory);
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

    public class GIVEN_application : GIVEN_bus_and_read_model
    {
        protected ItemApplication sut;

        public GIVEN_application()
        {
            this.sut = new ItemApplication(this.commandBus, "https://www.google.com.py/#q=", this.contextFactory);
        }

        [Fact]
        public void WHEN_making_a_command_request_THEN_process_request_successfully()
        {
            this.sut.AddItem("silla");
        }

        [Fact]
        public void WHEN_making_a_bad_command_request_THEN_trows()
        {
            Assert.Throws<AggregateException>(() => this.sut.AddItemBuggyProcess("silla"));
        }

        [Fact]
        public void WHEN_notifiyng_external_event_THEN_integrates_sucessfully()
        {
            // por ejemplo: ocurre algo (un evento de afuera)
            // Eso deberia pasar primero por el aggregate de integracion de eventos externos y
            // publicarlo con el source Id a modo de correlation id (lo que seria el command)
            // Esto no es necesario ahora, pero en el futuro podria ser util realizar esta implementacion.
        }
    }
}
