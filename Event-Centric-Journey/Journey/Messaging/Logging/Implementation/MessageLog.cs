using Journey.Messaging.Logging.Metadata;
using Journey.Serialization;
using Journey.Utils.SystemTime;
using Journey.Worker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Journey.Messaging.Logging
{
    public class MessageLog : MessageLogBase, IMessageAuditLog, IEventLogReader
    {
        private string connectionString;

        public MessageLog(string connectionString, ITextSerializer serializer, IMetadataProvider metadataProvider, IWorkerRoleTracer tracer, ISystemTime dateTime)
            : base(metadataProvider, serializer, tracer, dateTime)
        {
            this.connectionString = connectionString;
        }

        public void Log(IEvent @event)
        {
            using (var context = new MessageLogDbContext(this.connectionString))
            {
                var message = base.GetMessage(@event);

                // first lets check if is not a duplicated command message;
                var duplicatedMessage = context.Set<MessageLogEntity>()
                    .Where(m => m.SourceId.ToUpper() == message.SourceId.ToUpper()
                            && m.SourceType == message.SourceType
                            && m.Version == message.Version
                            && m.TypeName == message.TypeName)
                    .FirstOrDefault();

                if (duplicatedMessage != null)
                    return;

                // Is not duplicated...


                context.Set<MessageLogEntity>().Add(message);

                this.tracer.Trace(string.Format("Processing Event:\r\n{0}", message.Payload));

                context.SaveChanges();
            }
        }

        public void Log(ICommand command)
        {
            using (var context = new MessageLogDbContext(this.connectionString))
            {
                var message = base.GetMessage(command);

                // first lets check if is not a duplicated command message;
                var duplicatedMessage = context.Set<MessageLogEntity>()
                    .Where(m => m.SourceId.ToUpper() == message.SourceId.ToUpper())
                    .FirstOrDefault();

                if (duplicatedMessage != null)
                    return;

                // Is not duplicated...

                context.Set<MessageLogEntity>().Add(message);

                this.tracer.Trace(string.Format("Command processed!\r\n{0}", message.Payload));

                context.SaveChanges();
            }
        }

        public IEnumerable<IEvent> Query(EventLogQueryCriteria criteria)
        {
            return new SqlQuery(this.connectionString, this.serializer, criteria);
        }

        private class SqlQuery : IEnumerable<IEvent>
        {
            private string nameOrConnectionString;
            private ITextSerializer serializer;
            private EventLogQueryCriteria criteria;

            public SqlQuery(string nameOrConnectionString, ITextSerializer serializer, EventLogQueryCriteria criteria)
            {
                this.nameOrConnectionString = nameOrConnectionString;
                this.serializer = serializer;
                this.criteria = criteria;
            }

            public IEnumerator<IEvent> GetEnumerator()
            {
                return new DisposingEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private class DisposingEnumerator : IEnumerator<IEvent>
            {
                private SqlQuery sqlQuery;
                private MessageLogDbContext context;
                private IEnumerator<IEvent> events;

                public DisposingEnumerator(SqlQuery sqlQuery)
                {
                    this.sqlQuery = sqlQuery;
                }

                ~DisposingEnumerator()
                {
                    if (context != null) context.Dispose();
                }

                public void Dispose()
                {
                    if (context != null)
                    {
                        context.Dispose();
                        context = null;
                        GC.SuppressFinalize(this);
                    }
                    if (events != null)
                    {
                        events.Dispose();
                    }
                }

                public IEvent Current { get { return events.Current; } }
                object IEnumerator.Current { get { return this.Current; } }

                public bool MoveNext()
                {
                    if (context == null)
                    {
                        context = new MessageLogDbContext(sqlQuery.nameOrConnectionString);
                        var queryable = context.Set<MessageLogEntity>().AsQueryable()
                            .Where(x => x.Kind == StandardMetadata.EventKind);

                        var where = sqlQuery.criteria.ToExpression();
                        if (where != null)
                            queryable = queryable.Where(where);

                        events = queryable
                            .AsEnumerable()
                            .Select(x => this.sqlQuery.serializer.Deserialize<IEvent>(x.Payload))
                            .GetEnumerator();
                    }

                    return events.MoveNext();
                }

                public void Reset()
                {
                    throw new NotSupportedException();
                }
            }
        }


        public bool IsDuplicateMessage(IEvent @event)
        {
            // I did not implement because it is implemented in the log process.
            throw new NotImplementedException();
        }

        public bool IsDuplicateMessage(ICommand command)
        {
            // I did not implement because it is implemented in the log process.
            throw new NotImplementedException();
        }
    }
}
