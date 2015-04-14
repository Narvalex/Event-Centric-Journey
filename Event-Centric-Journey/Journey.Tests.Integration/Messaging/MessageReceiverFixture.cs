using Journey.Messaging;
using Journey.Utils.SystemDateTime;
using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Xunit;

namespace Journey.Tests.Integration.Messaging.MessageReceiverFixture
{
    public class GIVEN_sender_and_receiver : IDisposable
    {
        private readonly IDbConnectionFactory connectionFactory;
        private readonly MessageSender sender;
        private readonly TestableMessageReceiver receiver;
        private readonly string connectionString;
        private readonly string dbName = "TestSqlMessaging";

        public GIVEN_sender_and_receiver()
        {
            this.connectionFactory = System.Data.Entity.Database.DefaultConnectionFactory;
            this.sender = new MessageSender(this.connectionFactory, "TestSqlMessaging", "Test.Events");
            this.receiver = new TestableMessageReceiver(this.connectionFactory);

            this.connectionString = this.connectionFactory.CreateConnection(this.dbName).ConnectionString;
            MessagingDbInitializer.CreateDatabaseObjects(this.connectionString, "Test", true);
        }

        [Fact]
        public void WHEN_sending_confirmed_message_THEN_receives_message()
        {
            Message message = null;

            this.receiver.MessageReceived += (s, e) => { message = e.Message; };

            this.sender.Send(new Message("test message"));

            Assert.True(this.receiver.ReceiveMessage());
            Assert.Equal("test message", message.Body);
            Assert.Null(message.CorrelationId);
            Assert.Null(message.DeliveryDate);
        }

        //[Fact]
        //public void WHEN_sending_non_confirmed_message_THEN_does_not_receives_message()
        //{
        //    Message message = null;

        //    this.receiver.MessageReceived += (s, e) => { message = e.Message; };

        //    this.sender.Send(new Message("test message", null, null));

        //    Assert.False(this.receiver.ReceiveMessage());
        //    Assert.Null(message);
        //}

        //        [Fact]
        //        public void GIVEN_a_non_confirmed_message_already_sent_WHEN_message_is_confirmed_THEN_receives_message()
        //        {
        //            Message message = null;
        //            var correlationId = "correlation1";

        //            this.receiver.MessageReceived += (s, e) => { message = e.Message; };

        //            this.sender.Send(new Message("test message", correlationId, null, false));

        //            Assert.False(this.receiver.ReceiveMessage());
        //            Assert.Null(message);

        //            // Now we confirm the message for delivery
        //            var confirmMessageQuery = string.Format(@"
        //UPDATE [TestSqlMessaging].[Test].[Events] 
        //	SET [WasConfirmed] = 1
        //	WHERE [CorrelationId] = N'{0}'
        //", correlationId);

        //using (var connection = this.connectionFactory.CreateConnection(this.connectionString))
        //{
        //    connection.Open();

        //    using(var command = (SqlCommand)connection.CreateCommand())
        //    {
        //        command.CommandText = confirmMessageQuery;
        //        command.CommandType = System.Data.CommandType.Text;

        //        command.ExecuteNonQuery();
        //    }
        //}

        //    Assert.True(this.receiver.ReceiveMessage());
        //    Assert.Equal("test message", message.Body);
        //    Assert.Equal(correlationId, message.CorrelationId);
        //    Assert.Null(message.DeliveryDate);            			
        //}

        [Fact]
        public void WHEN_sending_message_with_correlation_id_THEN_receives_message()
        {
            Message message = null;

            this.receiver.MessageReceived += (s, e) => { message = e.Message; };

            this.sender.Send(new Message("test message", correlationId: "correlation"));

            Assert.True(this.receiver.ReceiveMessage());
            Assert.Equal("test message", message.Body);
            Assert.Equal("correlation", message.CorrelationId);
            Assert.Null(message.DeliveryDate);
        }

        [Fact]
        public void WHEN_successfully_handles_message_THEN_removes_message()
        {
            this.receiver.MessageReceived += (s, e) => { };

            this.sender.Send(new Message("test message"));

            Assert.True(this.receiver.ReceiveMessage());
            Assert.False(this.receiver.ReceiveMessage());
        }

        [Fact]
        public void WHEN_unsuccessfully_handles_message_THEN_does_not_remove_message_AND_mark_as_dead_letter()
        {
            EventHandler<MessageReceivedEventArgs> failureHandler = null;
            failureHandler = (s, e) => { this.receiver.MessageReceived -= failureHandler; throw new FakeMessageHandlingException(); };

            this.receiver.MessageReceived += failureHandler;

            this.sender.Send(new Message("test message"));

            try
            {
                Assert.True(this.receiver.ReceiveMessage());

                // El message receiver atrapa todos los errores de los handlers....

                //Assert.False(true, "should have thrown");
            }
            catch (FakeMessageHandlingException)
            { }

            // Message was marked as dead letter and is ignored
            Assert.False(this.receiver.ReceiveMessage());
        }

        [Fact]
        public void WHEN_sending_message_with_delay_THEN_receives_message_after_delay()
        {
            Message message = null;

            this.receiver.MessageReceived += (s, e) => { message = e.Message; };

            var deliveryDate = DateTime.Now.Add(TimeSpan.FromSeconds(2));
            this.sender.Send(new Message("test message", null, deliveryDate));

            Assert.False(this.receiver.ReceiveMessage());

            Thread.Sleep(TimeSpan.FromSeconds(6));

            Assert.True(this.receiver.ReceiveMessage());
            Assert.Equal("test message", message.Body);
        }

        /// <summary>
        /// Creo que es para probar el lock object
        /// </summary>
        [Fact]
        public void WHEN_receiving_message_THEN_other_receivers_cannot_see_message_but_see_other_messages()
        {
            var secondReceiver = new TestableMessageReceiver(this.connectionFactory);

            this.sender.Send(new Message("message1"));
            this.sender.Send(new Message("message2"));

            var waitEvent = new AutoResetEvent(false);
            string receiver1Message = null;
            string receiver2Message = null;

            this.receiver.MessageReceived += (s, e) =>
            {
                waitEvent.Set();
                receiver1Message = e.Message.Body;
                waitEvent.WaitOne();
            };
            secondReceiver.MessageReceived += (s, e) =>
            {
                receiver2Message = e.Message.Body;
            };

            ThreadPool.QueueUserWorkItem(_ => { this.receiver.ReceiveMessage(); });

            Assert.True(waitEvent.WaitOne(TimeSpan.FromSeconds(10)));
            secondReceiver.ReceiveMessage();
            waitEvent.Set();

            Assert.Equal("message1", receiver1Message);
            Assert.Equal("message2", receiver2Message);
        }

        [Fact]
        public void WHEN_receiving_message_THEN_can_send_new_message()
        {
            var secondReceiver = new TestableMessageReceiver(this.connectionFactory);

            this.sender.Send(new Message("message1"));

            var waitEvent = new AutoResetEvent(false);
            string receiver1Message = null;
            string receiver2Message = null;

            this.receiver.MessageReceived += (s, e) =>
            {
                waitEvent.Set();
                receiver1Message = e.Message.Body;
                waitEvent.WaitOne();
            };
            secondReceiver.MessageReceived += (s, e) =>
            {
                receiver2Message = e.Message.Body;
            };

            ThreadPool.QueueUserWorkItem(_ => { this.receiver.ReceiveMessage(); });

            Assert.True(waitEvent.WaitOne(TimeSpan.FromSeconds(10)));
            this.sender.Send(new Message("message2"));
            secondReceiver.ReceiveMessage();
            waitEvent.Set();

            Assert.Equal("message1", receiver1Message);
            Assert.Equal("message2", receiver2Message);
        }

        /// <summary>
        /// Disposes the Test Db.
        /// Implementation hint: http://stackoverflow.com/questions/11620/how-do-you-kill-all-current-connections-to-a-sql-server-2005-database
        /// </summary>
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
ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}') 
DROP DATABASE [{0}]
",
                            this.dbName);

                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public class TestableMessageReceiver : MessageReceiver
    {
        public TestableMessageReceiver(System.Data.Entity.Infrastructure.IDbConnectionFactory connectionFactory)
            : base(connectionFactory, "TestSqlMessaging", "Test.Events", TimeSpan.FromSeconds(1), 1, new LocalDateTime())
        {
        }

        public bool ReceiveMessage()
        {
            using (var connection = this.connectionFactory.CreateConnection(this.connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    return base.ReceiveMessage(connection, transaction);
                }
            }
        }
    }

    /// <summary>
    /// Este fake está para no opacar otro tipo de error.
    /// </summary>
    public class FakeMessageHandlingException : Exception
    { }
}
