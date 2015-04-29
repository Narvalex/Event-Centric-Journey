using Journey.Messaging;
using Journey.Messaging.Logging;
using Journey.Messaging.Logging.Metadata;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils;
using Journey.Utils.SystemDateTime;
using Journey.Worker;
using Journey.Worker.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Journey.EventSourcing.EventStoreRebuilding
{
    public class EventStoreRebuilderEngine : IEventStoreRebuilderEngine
    {
        private readonly EventStoreDbContext eventStoreContext;
        private readonly ITextSerializer serializer;
        private readonly IMetadataProvider metadataProvider;
        private readonly ISystemDateTime dateTime;

        private readonly IEventStoreRebuilderConfig config;

        private readonly IWorkerRoleTracer tracer;

        private readonly IInMemoryBus bus;

        private readonly IEventDispatcher eventDispatcher;
        private readonly ICommandProcessor commandProcessor;
        private readonly ICommandHandlerRegistry commandHandlerRegistry;

        private IMessageAuditLog auditLog;

        private MessageLogHandler handler;

        public EventStoreRebuilderEngine(
            IInMemoryBus bus,
            ICommandProcessor commandProcessor, ICommandHandlerRegistry commandHandlerRegistry, IEventDispatcher eventDispatcher,
            ITextSerializer serializer, IMetadataProvider metadataProvider,
            ISystemDateTime dateTime,
            IWorkerRoleTracer tracer,
            IEventStoreRebuilderConfig config,
            EventStoreDbContext eventStoreDbContext)
        {
            this.bus = bus;
            this.eventStoreContext = eventStoreDbContext;
            this.serializer = serializer;
            this.eventDispatcher = eventDispatcher;
            this.commandProcessor = commandProcessor;
            this.commandHandlerRegistry = commandHandlerRegistry;
            this.config = config;
            this.dateTime = dateTime;
            this.tracer = tracer;
            this.metadataProvider = metadataProvider;
        }

        public void Rebuild()
        {
            using (var sourceContext = new MessageLogDbContext(config.SourceMessageLogConnectionString))
            {
                var messages = sourceContext.Set<MessageLogEntity>()
                                .OrderBy(m => m.Id)
                                .AsEnumerable()
                                .Select(this.CreateMessage)
                                .AsCachedAnyEnumerable();

                using (var newContext = new MessageLogDbContext(config.NewMessageLogConnectionString))
                {
                    this.RegisterLogger(newContext);

                    this.ProcessMessages(messages);

                    // el borrado colocamos al final por si se este haciendo desde el mismo connection.
                    var result = newContext.Database.ExecuteSqlCommand("DELETE FROM [MessageLog].[Messages]");
                    newContext.SaveChanges();
                }
            }

            this.eventStoreContext.SaveChanges();
        }

        private void RegisterLogger(MessageLogDbContext newContext)
        {
            this.auditLog = new InMemoryMessageLog(this.serializer, this.metadataProvider, this.dateTime, this.tracer, newContext);
            this.handler = new MessageLogHandler(this.auditLog);
            this.commandHandlerRegistry.Register(this.handler);
            this.eventDispatcher.Register(this.handler);
        }

        private void ProcessMessages(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                var body = this.Deserialize(message.Body);

                var command = body as ICommand;
                if (command != null)
                    this.ProcessCommand(command);
                else
                    this.ProcessEvent(body as IEvent);
            }
        }

        private void ProcessCommand(ICommand command)
        {
            if (this.auditLog.IsDuplicateMessage(command))
                return;

            this.commandProcessor.ProcessMessage(command);
            this.ProcessInnerMessages();
        }

        private void ProcessInnerMessages()
        {
            if (this.bus.HasNewCommands)
                foreach (var command in bus.GetCommands())
                    this.ProcessCommand(command);

            if (this.bus.HasNewEvents)
                foreach (var @event in bus.GetEvents())
                    this.ProcessEvent(@event);
        }

        private void ProcessEvent(IEvent @event)
        {
            if (this.auditLog.IsDuplicateMessage(@event))
                return;

            this.eventDispatcher.DispatchMessage(@event, null, string.Empty, string.Empty);
            this.ProcessInnerMessages();
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
