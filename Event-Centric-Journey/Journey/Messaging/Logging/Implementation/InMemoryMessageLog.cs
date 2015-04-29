using Journey.Messaging.Logging.Metadata;
using Journey.Serialization;
using Journey.Utils;
using Journey.Utils.SystemDateTime;
using Journey.Worker;
using System;
using System.Linq;

namespace Journey.Messaging.Logging
{
    public class InMemoryMessageLog : IMessageAuditLog
    {
        private readonly MessageLogDbContext context;
        private readonly IMetadataProvider metadataProvider;
        private readonly ITextSerializer serializer;
        private readonly ISystemDateTime dateTime;
        private readonly IWorkerRoleTracer tracer;

        public InMemoryMessageLog(ITextSerializer serializer, IMetadataProvider metadataProvider, ISystemDateTime dateTime, IWorkerRoleTracer tracer, MessageLogDbContext context)
        {
            this.serializer = serializer;
            this.metadataProvider = metadataProvider;
            this.dateTime = dateTime;
            this.tracer = tracer;
            this.context = context;
        }

        // REFACTORIZAR EN UN METODO PARA ESTIRAR LA ENTIDAD DE MENSAJE Y ASI USARLO PARA 
        // VERIFICAR SI ES DUPLICADO
        public void Log(IEvent @event)
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
        }

        public void Log(ICommand command)
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
        }

        public bool IsDuplicateMessage(IEvent @event)
        {
            throw new NotImplementedException();
        }

        public bool IsDuplicateMessage(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}
