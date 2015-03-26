using Journey.EventSourcing;
using Journey.Messaging.Logging.Metadata;
using Journey.Serialization;
using Journey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Journey.Messaging.Logging
{
    public class MessageLog : IEventLogReader
 {
        private string connectionString;
        private IMetadataProvider metadataProvider;
        private ITextSerializer serializer;

        public MessageLog(string connectionString, ITextSerializer serializer, IMetadataProvider metadataProvider)
        {
            this.connectionString = connectionString;
            this.serializer = serializer;
            this.metadataProvider = metadataProvider;
        }

        public void Save(IEvent @event)
        {
            using (var context = new MessageLogDbContext(this.connectionString))
            {
                // first lets check if is not a duplicated command message;
                var duplicatedMessage = context.Set<MessageLogEntity>()
                    .Where(m => m.SourceId.ToUpper() == @event.SourceId.ToString().ToUpper()
                            && m.Version == ((ITraceableVersionedEvent)@event).Version.ToString())
                    .FirstOrDefault();

                if (duplicatedMessage != null)
                    return;

                // Is not duplicated...
                var metadata = this.metadataProvider.GetMetadata(@event);

                context.Set<MessageLogEntity>().Add(new MessageLogEntity
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
                    CreationDate = DateTime.Now.ToString("o"),
                    Payload = serializer.Serialize(@event),
                });
                context.SaveChanges();
            }
        }

        public void Save(ICommand command)
        {
            using (var context = new MessageLogDbContext(this.connectionString))
            {
                // first lets check if is not a duplicated command message;
                var duplicatedMessage = context.Set<MessageLogEntity>()
                    .Where(m => m.SourceId.ToUpper() == command.Id.ToString().ToUpper())
                    .FirstOrDefault();

                if (duplicatedMessage != null)
                    return;

                // Is not duplicated...
                var metadata = this.metadataProvider.GetMetadata(command);

                context.Set<MessageLogEntity>().Add(new MessageLogEntity
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
                    CreationDate = DateTime.Now.ToString("o"),
                    Payload = serializer.Serialize(command),
                });
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
