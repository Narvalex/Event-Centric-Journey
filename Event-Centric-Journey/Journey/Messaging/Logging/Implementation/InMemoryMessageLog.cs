using Journey.Messaging.Logging.Metadata;
using Journey.Serialization;
using Journey.Utils.SystemTime;
using Journey.Worker;
using System.Linq;

namespace Journey.Messaging.Logging
{
    public class InMemoryMessageLog : MessageLogBase, IMessageAuditLog
    {
        private readonly MessageLogDbContext context;

        public InMemoryMessageLog(ITextSerializer serializer, IMetadataProvider metadataProvider, IWorkerRoleTracer tracer, MessageLogDbContext context, ISystemTime dateTime)
            : base(metadataProvider, serializer, tracer, dateTime)
        {
            this.context = context;
        }

        public void Log(IEvent @event)
        {
            var message = GetMessage(@event);
            context.Set<MessageLogEntity>().Add(message);
            this.tracer.Notify(string.Format("Processing Event:\r\n{0}", message.Payload));
        }

        public void Log(ICommand command)
        {
            var message = GetMessage(command);
            context.Set<MessageLogEntity>().Add(message);
            this.tracer.Notify(string.Format("Command processed!\r\n{0}", message.Payload));
        }

        public bool IsDuplicateMessage(IEvent @event)
        {
            var message = GetMessage(@event);

            return context.Set<MessageLogEntity>()
                .Local
                .Where(m => m.SourceId.ToUpper() == message.SourceId.ToUpper()
                        && m.SourceType == message.SourceType
                        && m.Version == message.Version
                        && m.TypeName == message.TypeName)
                .Any();
        }

        public bool IsDuplicateMessage(ICommand command)
        {
            var message = GetMessage(command);

            return context.Set<MessageLogEntity>()
                .Local
                .Where(m => m.SourceId.ToUpper() == message.SourceId.ToUpper())
                .Any();
        }
    }
}
