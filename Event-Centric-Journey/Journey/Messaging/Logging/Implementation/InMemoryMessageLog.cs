using Journey.Messaging.Logging.Metadata;
using Journey.Serialization;
using Journey.Utils;
using Journey.Worker;
using System.Linq;

namespace Journey.Messaging.Logging
{
    public class InMemoryMessageLog : IMessageAuditLog
    {
        private readonly MessageLogDbContext context;
        private readonly IMetadataProvider metadataProvider;
        private readonly ITextSerializer serializer;
        private readonly IWorkerRoleTracer tracer;

        public InMemoryMessageLog(ITextSerializer serializer, IMetadataProvider metadataProvider, IWorkerRoleTracer tracer, MessageLogDbContext context)
        {
            this.serializer = serializer;
            this.metadataProvider = metadataProvider;
            this.tracer = tracer;
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

        private MessageLogEntity GetMessage(IEvent @event)
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
                CreationDate = @event.CreationDate.ToString("o"),
                Payload = serializer.Serialize(@event),
            };

            return message;
        }

        private MessageLogEntity GetMessage(ICommand command)
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
                CreationDate = command.CreationDate.ToString("o"),
                Payload = serializer.Serialize(command),
            };

            return message;
        }
    }
}
