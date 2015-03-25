using Journey.Messaging;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Globalization;
using Xunit;

namespace Journey.Tests.Integration.Messaging.MessageSenderFixture
{
    public class GIVEN_sender : IDisposable
    {
        private IDbConnectionFactory connectionFactory;
        private IMessageSender sender;
        private readonly string connectionString;
        private readonly string dbName = "TestSqlMessaging";

        public GIVEN_sender()
        {
            this.connectionFactory = System.Data.Entity.Database.DefaultConnectionFactory;
            this.sender = new MessageSender(this.connectionFactory, this.dbName, "Test.Commands");

            this.connectionString = this.connectionFactory.CreateConnection(this.dbName).ConnectionString;
            MessagingDbInitializer.CreateDatabaseObjects(this.connectionString, "Test", true);
        }

        [Fact]
        public void WHEN_sending_string_message_THEN_saves_message()
        {
            var messageWasSaved = false;
            var messageBody = "Message-" + Guid.NewGuid().ToString();
            var message = new Message(messageBody);

            this.sender.Send(message);


            using (var connection = this.connectionFactory.CreateConnection("TestSqlMessaging"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = string.Format(CultureInfo.InvariantCulture, @"
SELECT [Body]
  FROM [Test].[Commands]
  WHERE BODY = N'{0}'
", messageBody);

                using (var reader = command.ExecuteReader())
                {
                    messageWasSaved = reader.Read();
                }
            }

            Assert.True(messageWasSaved);
        }

        //        [Fact]
        //        public void WHEN_sending_a_comfirmed_string_message_THEN_saves_comfirmed_message()
        //        {
        //            var messageWasSaved = false;
        //            var messageBody = "Message-" + Guid.NewGuid().ToString();
        //            var message = new Message(messageBody);

        //            this.sender.Send(message);


        //            using (var connection = this.connectionFactory.CreateConnection("TestSqlMessaging"))
        //            {
        //                connection.Open();
        //                var command = connection.CreateCommand();
        //                command.CommandText = string.Format(CultureInfo.InvariantCulture, @"
        //SELECT [Body]
        //  FROM [Test].[Commands]
        //  WHERE BODY = N'{0}'
        //  AND WasConfirmed = 1
        //", messageBody);

        //                using (var reader = command.ExecuteReader())
        //                {
        //                    messageWasSaved = reader.Read();
        //                }
        //            }

        //            Assert.True(messageWasSaved);
        //        }

        //        [Fact]
        //        public void WHEN_sending_non_comfirmed_string_message_THEN_saves_non_comfirmed_message()
        //        {
        //            var messageWasSaved = false;
        //            var messageBody = "Message-" + Guid.NewGuid().ToString();
        //            var message = new Message(messageBody, null, null, false);

        //            this.sender.Send(message);


        //            using (var connection = this.connectionFactory.CreateConnection("TestSqlMessaging"))
        //            {
        //                connection.Open();
        //                var command = connection.CreateCommand();
        //                command.CommandText = string.Format(CultureInfo.InvariantCulture, @"
        //SELECT [Body]
        //  FROM [Test].[Commands]
        //  WHERE BODY = N'{0}'
        //  AND WasConfirmed = 0
        //", messageBody);

        //                using (var reader = command.ExecuteReader())
        //                {
        //                    messageWasSaved = reader.Read();
        //                }
        //            }

        //            Assert.True(messageWasSaved);
        //        }

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
ALTER DATABASE TestSqlMessaging SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'TestSqlMessaging') 
DROP DATABASE [TestSqlMessaging]
",
                            this.dbName);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
