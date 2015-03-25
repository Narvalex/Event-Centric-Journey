using Infrastructure.CQRS.Messaging;
using Infrastructure.CQRS.Messaging.Logging;
using Infrastructure.CQRS.Messaging.Processing;
using Infrastructure.CQRS.Serialization;
using Infrastructure.CQRS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Infrastructure.CQRS.EventSourcing.Rebuilding
{
    public class EventStoreRebuilder : IEventStoreRebuilder
    {
        private readonly Func<MessageLogDbContext> messageLogDbContextFactory;
        ITextSerializer serializer;

        private readonly IEventDispatcher eventDispatcher;
        private readonly ICommandProcessor commandProcessor;

        public EventStoreRebuilder(ICommandProcessor commandProcessor, IEventDispatcher eventDispatcher, ITextSerializer serializer, Func<MessageLogDbContext> messageLogDbContextFactory)
        {
            this.serializer = serializer;
            this.eventDispatcher = eventDispatcher;
            this.commandProcessor = commandProcessor;
            this.messageLogDbContextFactory = messageLogDbContextFactory;
        }

        public void Rebuild(EventStoreDbContext eventStoreDbContext)
        {
            using (var context = this.messageLogDbContextFactory.Invoke())
            {
                var messages = context.Set<MessageLogEntity>()
                                .OrderBy(m => m.Id)
                                .AsEnumerable()
                                .Select(this.CreateMessage)
                                .AsCachedAnyEnumerable();

                this.ProcessMessages(messages);
            }

            eventStoreDbContext.SaveChanges();
        }

        private void ProcessMessages(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                var body = this.Deserialize(message.Body);

                var @event = body as IEvent;
                if (@event != null)
                    this.eventDispatcher.DispatchMessage(@event, null, string.Empty, string.Empty);

                var command = body as ICommand;
                if (command != null)
                    this.commandProcessor.ProcessMessage(body);
            }
        }

        private Message CreateMessage(MessageLogEntity message)
        {
            return new Message(message.Payload);
        }

        private object Deserialize(string serializedPayload)
        {
            using (var reader = new StringReader(serializedPayload))
            {
                return this.serializer.Deserialize(reader);
            }
        }
    }
}
