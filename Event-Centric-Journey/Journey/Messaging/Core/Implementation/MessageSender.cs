using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;

namespace Journey.Messaging
{
    public class MessageSender : IMessageSender, ISqlBus
    {
        private readonly IDbConnectionFactory connectionFactory;
        private readonly string dbName;
        private readonly string insertQuery;

        private MessageSender(string tableName)
        {
            this.TableName = tableName;
        }

        /// <summary>
        /// Creates a new instance of <see cref="MessageSender"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connectionFactory.</param>
        /// <param name="dbName">The database name.</param>
        /// <param name="tableName">The table name of the bus to send messages.</param>
        public MessageSender(IDbConnectionFactory connectionFactory, string dbName, string tableName)
            : this(tableName)
        {
            this.connectionFactory = connectionFactory;
            this.dbName = dbName;
            this.insertQuery = string.Format(@"
INSERT INTO {0} 
(Body, DeliveryDate, CorrelationId, IsDeadLetter) 
VALUES 
(@Body, @DeliveryDate, @CorrelationId, 0)", this.TableName);
        }

        public string TableName { get; private set; }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        public void Send(Message message)
        {
            using (var connection = this.connectionFactory.CreateConnection(this.dbName))
            {
                connection.Open();

                this.InsertMessage(message, connection);
            }
        }

        /// <summary>
        /// Sends a batch of messages.
        /// </summary>
        public void Send(IEnumerable<Message> messages)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                using (var connection = this.connectionFactory.CreateConnection(this.dbName))
                {
                    connection.Open();

                    foreach (var message in messages)
                    {
                        this.InsertMessage(message, connection);
                    }
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// Reliably sends a batch of messages.
        /// </summary>
        public void Send(IEnumerable<Message> messages, DbContext context)
        {
            foreach (var message in messages)
                this.ReliablyInsertMessage(message, context);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Does not contain user input.")]
        private void InsertMessage(Message message, DbConnection connection)
        {
            using (var command = (SqlCommand)connection.CreateCommand())
            {
                command.CommandText = this.insertQuery;
                command.CommandType = CommandType.Text;

                command.Parameters.Add("@Body", SqlDbType.NVarChar).Value = message.Body;
                command.Parameters.Add("@DeliveryDate", SqlDbType.DateTime).Value = message.DeliveryDate.HasValue ? (object)message.DeliveryDate.Value : DBNull.Value;
                command.Parameters.Add("@CorrelationId", SqlDbType.NVarChar).Value = (object)message.CorrelationId ?? DBNull.Value;

                command.ExecuteNonQuery();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Does not contain user input.")]
        private void ReliablyInsertMessage(Message message, DbContext context)
        {
            context.Database.ExecuteSqlCommand(this.insertQuery,
                new SqlParameter("@Body", message.Body),
                new SqlParameter("@DeliveryDate", message.DeliveryDate.HasValue ? (object)message.DeliveryDate.Value : DBNull.Value),
                new SqlParameter("@CorrelationId", (object)message.CorrelationId ?? DBNull.Value));
        }
    }
}
