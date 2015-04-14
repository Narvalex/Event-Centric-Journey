using Journey.Utils.SystemDateTime;
using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Journey.Messaging
{
    public class MessageReceiver : IMessageReceiver, IDisposable
    {
        protected readonly IDbConnectionFactory connectionFactory;
        protected readonly string connectionString;
        private readonly string readQuery;
        private readonly string deleteQuery;
        private readonly string setDeadLetterQuery;
        private readonly int numberOfThreads;
        private readonly TimeSpan pollDelay;
        private readonly object lockObject = new object();
        private CancellationTokenSource cancellationSource;
        private Action delegateMessageReceiving;
        private readonly ISystemDateTime dateTime;

        public MessageReceiver(IDbConnectionFactory connectionFactory, string connectionString, string tableName, TimeSpan busPollDelay, int numberOfThreads, ISystemDateTime dateTime)
        {
            this.connectionFactory = connectionFactory;
            this.connectionString = connectionString;
            this.pollDelay = busPollDelay;
            this.numberOfThreads = numberOfThreads;
            this.dateTime = dateTime;

            this.readQuery =
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"SELECT TOP (1) 
                    {0}.[Id] AS [Id], 
                    {0}.[Body] AS [Body], 
                    {0}.[DeliveryDate] AS [DeliveryDate],
                    {0}.[CorrelationId] AS [CorrelationId]
                    FROM {0} WITH (UPDLOCK, READPAST)
                    WHERE (({0}.[DeliveryDate] IS NULL) OR ({0}.[DeliveryDate] <= @CurrentDate))
                            AND {0}.[IsDeadLetter] = 0
                    ORDER BY {0}.[Id] ASC",
                    tableName);

            this.deleteQuery =
                string.Format(
                   CultureInfo.InvariantCulture,
                   "DELETE FROM {0} WHERE Id = @Id AND IsDeadLetter = 0",
                   tableName);

            this.setDeadLetterQuery =
                string.Format(CultureInfo.InvariantCulture,
                @"update {0} set
                {0}.IsDeadLetter = 1,
                {0}.TraceInfo = @TraceInfo
                where {0}.Id = @Id",
                tableName);

            this.delegateMessageReceiving = () => { };
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = (sender, args) => { };

        public void Start()
        {
            lock (this.lockObject)
            {
                if (this.cancellationSource == null)
                {
                    this.cancellationSource = new CancellationTokenSource();
                    this.delegateMessageReceiving = () => this.StartNewMessageReceiver();

                    for (int i = 0; i < this.numberOfThreads; i++)
                        this.StartNewMessageReceiver();
                }
            }
        }


        private void StartNewMessageReceiver()
        {
            Task.Factory.StartNew(() =>
                this.ReceiveMessages(this.cancellationSource.Token),
                this.cancellationSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Current);
        }

        public void Stop()
        {
            lock (this.lockObject)
            {
                using (this.cancellationSource)
                {
                    if (this.cancellationSource != null)
                    {
                        this.cancellationSource.Cancel();
                        this.delegateMessageReceiving = () => { };
                        this.cancellationSource = null;
                    }
                }
            }
        }

        private void ReceiveMessages(CancellationToken cancellationToken)
        {
            using (var connection = this.connectionFactory.CreateConnection(this.connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    while (!cancellationToken.IsCancellationRequested)
                        if (!this.ReceiveMessage(connection, transaction))
                            Thread.Sleep(this.pollDelay);
                        else
                            break;
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Does not contain user input.")]
        protected bool ReceiveMessage(DbConnection connection, DbTransaction transaction)
        {
            var currentDate = this.GetCurrentDate();

            long messageId = -1;
            Message message = null;

            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = this.readQuery;
                    ((SqlCommand)command).Parameters.Add("@CurrentDate", SqlDbType.DateTime).Value = currentDate;

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }

                        var body = (string)reader["Body"];
                        var deliveryDateValue = reader["DeliveryDate"];
                        var deliveryDate = deliveryDateValue == DBNull.Value ? (DateTime?)null : new DateTime?((DateTime)deliveryDateValue);
                        var correlationIdValue = reader["CorrelationId"];
                        var correlationId = (string)(correlationIdValue == DBNull.Value ? null : correlationIdValue);

                        message = new Message(body, correlationId, deliveryDate);
                        messageId = (long)reader["Id"];
                    }
                }

                this.delegateMessageReceiving.Invoke();
                this.MessageReceived(this, new MessageReceivedEventArgs(message));

                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;
                    command.CommandText = this.deleteQuery;
                    ((SqlCommand)command).Parameters.Add("@Id", SqlDbType.BigInt).Value = messageId;

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception e)
            {
                try
                {
                    // Dead Lettering
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandType = CommandType.Text;
                        command.CommandText = this.setDeadLetterQuery;
                        ((SqlCommand)command).Parameters.Add("@Id", SqlDbType.BigInt).Value = messageId;
                        ((SqlCommand)command).Parameters.Add("@TraceInfo", SqlDbType.NVarChar).Value =
                            string.Format("Exception Type: {0}. Exception Message: {1} Inner Exception Message: {2} StackTrace: {3}", e.GetType().Name, e.Message, e.InnerException, e.StackTrace);

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception)
                    {
                        // NOTE: we catch ANY exceptions. This implementation 
                        // supports retries and dead-lettering.
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Esto parece que es para que funcione los mensajes que tienen Delay, como los que son por 
        /// tiempo. 
        /// </summary>
        protected virtual DateTime GetCurrentDate()
        {
            return this.dateTime.Now;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Stop();
        }

        ~MessageReceiver()
        {
            Dispose(false);
        }
    }
}
