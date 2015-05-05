using Journey.Messaging.Logging.Metadata;
using Journey.Serialization;
using Journey.Utils;
using Journey.Utils.SystemDateTime;
using Journey.Worker;
using System;

namespace Journey.Messaging.Logging
{
    public abstract class MessageLogBase
    {
        protected readonly IMetadataProvider metadataProvider;
        protected readonly ITextSerializer serializer;
        protected readonly IWorkerRoleTracer tracer;
        protected Func<string> lastUpdateTimeProvider;

        public MessageLogBase(IMetadataProvider metadataProvider, ITextSerializer serializer, IWorkerRoleTracer tracer, ISystemDateTime dateTime)
        {
            this.metadataProvider = metadataProvider;
            this.serializer = serializer;
            this.tracer = tracer;
            this.lastUpdateTimeProvider = () => dateTime.Now.ToString("o");
        }

        protected MessageLogEntity GetMessage(IEvent @event)
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
                LastUpdateTime = lastUpdateTimeProvider.Invoke(),
                Payload = serializer.Serialize(@event),
            };

            return message;
        }

        protected MessageLogEntity GetMessage(ICommand command)
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
                LastUpdateTime = this.lastUpdateTimeProvider.Invoke(),
                Payload = serializer.Serialize(command),
            };

            return message;
        }
    }
}
