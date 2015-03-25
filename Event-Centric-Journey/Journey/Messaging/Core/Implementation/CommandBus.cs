using Infrastructure.CQRS.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace Infrastructure.CQRS.Messaging
{
    /// <summary>
    /// A command bus taht sends serialized object payloads thorugh a <see cref="IMessageSender"/>.
    /// </summary>
    public class CommandBus : SqlBus, ICommandBus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBus"/> class.
        /// </summary>
        /// <param name="serializer">The serializer to use for the message body.</param>
        public CommandBus(IMessageSender sender, ITextSerializer serializer)
            : base(sender, serializer)
        { }

        /// <summary>
        /// Sends the specified command.
        /// </summary>
        public void Send(Envelope<ICommand> command)
        {
            var message = BuildMessage(command);

            this.sender.Send(message);
        }

        /// <summary>
        /// Sends the specified commands.
        /// </summary>
        public void Send(IEnumerable<Envelope<ICommand>> commands)
        {
            var messages = commands.Select(command => BuildMessage(command));

            this.sender.Send(messages);
        }

        /// <summary>
        /// Reliably sends the specified commands whithin an event sourcing process.
        /// </summary>
        public void Send(IEnumerable<Envelope<ICommand>> commands, DbContext context)
        {
            var messages = commands.Select(command => BuildMessage(command));

            this.sender.Send(messages, context);
        }

        private Message BuildMessage(Envelope<ICommand> envelopedCommand)
        {
            // TODO: should use the Command ID as a unique constraint when storing it.
            using (var payloadWriter = new StringWriter())
            {
                this.serializer.Serialize(payloadWriter, envelopedCommand.Body);
                return new Message(payloadWriter.ToString(), envelopedCommand.CorrelationId, envelopedCommand.Delay != TimeSpan.Zero ? (DateTime?)DateTime.Now.Add(envelopedCommand.Delay) : null);
            }
        }
    }
}
