using Journey.Serialization;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace Journey.Messaging
{
    /// <summary>
    /// An event bus that sends serialized object payloads through a <see cref="IMessageSender"/>.
    /// This is an extremely basic implementation of <see cref="IEventBus"/>.
    /// </summary>
    public class ClientEventBus : SqlBus, IEventBus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventBus"/> class.
        /// </summary>
        /// <param name="serializer">The serializer to use for the message body.</param>
        public ClientEventBus(IMessageSender sender, ITextSerializer serializer)
            : base(sender, serializer)
        { }

        /// <summary>
        /// Sends the specified event.
        /// </summary>
        public void Publish(Envelope<IEvent> @event)
        {
            var message = this.BuildMessage(@event);

            this.sender.Send(message);
        }

        /// <summary>
        /// Publishes the specified events.
        /// </summary>
        public void Publish(IEnumerable<Envelope<IEvent>> events)
        {
            var messages = events.Select(e => this.BuildMessage(e));

            this.sender.Send(messages);
        }

        /// <summary>
        /// Reliably publishes the specified events.
        /// </summary>
        public void Publish(IEnumerable<Envelope<IEvent>> events, DbContext context)
        {
            var messages = events.Select(e => this.BuildMessage(e));

            this.sender.Send(messages, context);
        }

        private Message BuildMessage(Envelope<IEvent> @event)
        {
            using (var payloadWriter = new StringWriter())
            {
                this.serializer.Serialize(payloadWriter, @event.Body);
                return new Message(payloadWriter.ToString(), correlationId: @event.CorrelationId);
            }
        }
    }
}
