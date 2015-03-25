using Infrastructure.CQRS.Database;
using Infrastructure.CQRS.Messaging.Logging;
using Infrastructure.CQRS.Messaging.Logging.Metadata;
using Infrastructure.CQRS.Serialization;
using Infrastructure.CQRS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Infrastructure.CQRS.Messaging.Implementation
{
    /// <summary>
    /// A command bus taht sends serialized object payloads thorugh a <see cref="IMessageSender"/>.
    /// </summary>
    public class ClientCommandBus : SqlBus, IClientCommandBus
    {
        private readonly IConnectionStringProvider connectionProvider;
        private readonly IMetadataProvider metadataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCommandBus"/> class.
        /// </summary>
        /// <param name="serializer">The serializer to use for the message body.</param>
        public ClientCommandBus(IMessageSender sender, ITextSerializer serializer, IConnectionStringProvider connectionProvider, IMetadataProvider metadataProvider)
            : base(sender, serializer)
        {
            this.connectionProvider = connectionProvider;
            this.metadataProvider = metadataProvider;
        }

        /// <summary>
        /// Sends the specified command.
        /// </summary>
        public void Send(Envelope<ICommand> command)
        {
            var uniqueMessage = new Envelope<ICommand>[1] { command };
            this.Send(uniqueMessage);
        }

        /// <summary>
        /// Sends the specified commands.
        /// </summary>
        public void Send(IEnumerable<Envelope<ICommand>> commands)
        {
            using (var context = new MessageLogDbContext(this.connectionProvider.ConnectionString))
            {
                var messages = commands.Select(command => BuildAndLogMessage(command, context));

            
                this.sender.Send(messages, context);

                context.SaveChanges();
            } 
        }

        private Message BuildAndLogMessage(Envelope<ICommand> envelopedCommand, MessageLogDbContext context)
        {
            // Logging
            var command = envelopedCommand.Body;

            var metadata = this.metadataProvider.GetMetadata(command);

            context.Set<MessageLogEntity>().Add(new MessageLogEntity
            {
                //Id = Guid.NewGuid(),
                SourceId = command.Id.ToString(),
                Kind = metadata.TryGetValue(StandardMetadata.Kind),
                AssemblyName = metadata.TryGetValue(StandardMetadata.AssemblyName),
                FullName = metadata.TryGetValue(StandardMetadata.FullName),
                Namespace = metadata.TryGetValue(StandardMetadata.Namespace),
                TypeName = metadata.TryGetValue(StandardMetadata.TypeName),
                SourceType = metadata.TryGetValue(StandardMetadata.SourceType) as string,
                CreationDate = DateTime.Now.ToString("o"),
                Payload = serializer.Serialize(command),
            });

            // TODO: should use the Command ID as a unique constraint when storing it.
            using (var payloadWriter = new StringWriter())
            {
                this.serializer.Serialize(payloadWriter, envelopedCommand.Body);
                return new Message(payloadWriter.ToString(), envelopedCommand.CorrelationId, envelopedCommand.Delay != TimeSpan.Zero ? (DateTime?)DateTime.Now.Add(envelopedCommand.Delay) : null);
            }
        }
    }
}
