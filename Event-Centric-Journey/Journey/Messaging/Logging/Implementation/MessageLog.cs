using Journey.Messaging.Logging.Metadata;
using Journey.Serialization;
using Journey.Utils;
using Journey.Utils.SystemDateTime;
using Journey.Worker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Journey.Messaging.Logging
{
    public class MessageLog : IMessageLogger, IEventLogReader
 {
        private string connectionString;
        private readonly IMetadataProvider metadataProvider;
        private readonly ITextSerializer serializer;
        private readonly ISystemDateTime dateTime;
        private readonly IWorkerRoleTracer tracer;

        public MessageLog(string connectionString, ITextSerializer serializer, IMetadataProvider metadataProvider, ISystemDateTime dateTime, IWorkerRoleTracer tracer)
        {
            this.connectionString = connectionString;
            this.serializer = serializer;
            this.metadataProvider = metadataProvider;
            this.dateTime = dateTime;
            this.tracer = tracer;
        }

        public void Log(IEvent @event)
        {
            using (var context = new MessageLogDbContext(this.connectionString))
            {
                var metadata = this.metadataProvider.GetMetadata(@event);                

                var message = new MessageLogEntity
                {
                    //Id = Guid.NewGuid(),
                    SourceId = metadata.TryGetValue(StandardMetadata.SourceId),
                    Version = metadata.TryGetValue(StandardMetadata.Version),
                    Kind = metadata.TryGetValue(StandardMetadata.Kind),
                    AssemblyName = metadata.TryGetValue(StandardMetadata.AssemblyName),
                    FullName = metadata.TryGetValue(StandardMetadata.FullName),
                    Namespace = metadata.TryGetValue(StandardMetadata.Namespace),
                    TypeName = metadata.TryGetValue(StandardMetadata.TypeName),
                    SourceType = metadata.TryGetValue(StandardMetadata.SourceType),
                    CreationDate = this.dateTime.Now.ToString("o"),
                    Payload = serializer.Serialize(@event),
                };


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

                this.tracer.Notify(string.Format("Processing Event:\r\n{0}", message.Payload));

                context.SaveChanges();
            }
        }

        public void Log(ICommand command)
        {
            using (var context = new MessageLogDbContext(this.connectionString))
            {
                var metadata = this.metadataProvider.GetMetadata(command);                

                var message = new MessageLogEntity
                {
                    //Id = Guid.NewGuid(),
                    SourceId = metadata.TryGetValue(StandardMetadata.SourceId),
                    Version = metadata.TryGetValue(StandardMetadata.Version),
                    Kind = metadata.TryGetValue(StandardMetadata.Kind),
                    AssemblyName = metadata.TryGetValue(StandardMetadata.AssemblyName),
                    FullName = metadata.TryGetValue(StandardMetadata.FullName),
                    Namespace = metadata.TryGetValue(StandardMetadata.Namespace),
                    TypeName = metadata.TryGetValue(StandardMetadata.TypeName),
                    SourceType = metadata.TryGetValue(StandardMetadata.SourceType),
                    CreationDate = this.dateTime.Now.ToString("o"),
                    Payload = serializer.Serialize(command),
                };

                // first lets check if is not a duplicated command message;
                var duplicatedMessage = context.Set<MessageLogEntity>()
                    .Where(m => m.SourceId.ToUpper() == message.SourceId.ToUpper())
                    .FirstOrDefault();

                if (duplicatedMessage != null)
                    return;

                // Is not duplicated...

                context.Set<MessageLogEntity>().Add(message);

                this.tracer.Notify(string.Format("Command processed!\r\n{0}", message.Payload));

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
    }
}
