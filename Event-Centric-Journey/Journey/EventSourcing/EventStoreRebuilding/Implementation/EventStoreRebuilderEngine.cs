using Journey.Database;
using Journey.EventSourcing.RebuildPerfCounting;
using Journey.Messaging;
using Journey.Messaging.Logging;
using Journey.Messaging.Logging.Metadata;
using Journey.Messaging.Processing;
using Journey.Serialization;
using Journey.Utils;
using Journey.Worker;
using Journey.Worker.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Journey.EventSourcing.EventStoreRebuilding
{
    public class EventStoreRebuilderEngine : IEventStoreRebuilderEngine
    {
        private readonly Func<EventStoreDbContext> sourceContextFactory;
        private readonly Func<EventStoreDbContext> newContextFactory;

        private readonly Func<ICommand, bool> isDuplicateCommand;
        private readonly Func<IEvent, bool> isDuplicateEvent;

        private readonly ITextSerializer serializer;
        private readonly IMetadataProvider metadataProvider;

        private readonly IEventStoreRebuilderConfig config;

        private readonly ITracer tracer;

        private readonly IInMemoryBus bus;

        private readonly IEventDispatcher eventDispatcher;
        private readonly ICommandProcessor commandProcessor;
        private readonly ICommandHandlerRegistry commandHandlerRegistry;

        private readonly IRebuilderPerfCounter perfCounter;

        public EventStoreRebuilderEngine(
            IInMemoryBus bus,
            ICommandProcessor commandProcessor, ICommandHandlerRegistry commandHandlerRegistry, IEventDispatcher eventDispatcher,
            ITextSerializer serializer, IMetadataProvider metadataProvider,
            ITracer tracer,
            IEventStoreRebuilderConfig config,
            IRebuilderPerfCounter perfCounter)
        {
            this.bus = bus;
            this.serializer = serializer;
            this.eventDispatcher = eventDispatcher;
            this.commandProcessor = commandProcessor;
            this.commandHandlerRegistry = commandHandlerRegistry;
            this.config = config;
            this.tracer = tracer;
            this.metadataProvider = metadataProvider;
            this.perfCounter = perfCounter;

            this.sourceContextFactory = () =>
            {
                var context = new EventStoreDbContext(this.config.SourceEventStoreConnectionString);
                context.Configuration.AutoDetectChangesEnabled = false;
                return context;
            };
            this.newContextFactory = () => new EventStoreDbContext(this.config.NewEventStoreConnectionString);
        }

        public void Rebuild()
        {
            var rowsAffected = default(int);

            this.perfCounter.OnStartingRebuildProcess(this.GetMessagesCount());
            this.perfCounter.OnOpeningDbConnectionAndCleaning();

            using (var newContext = this.newContextFactory.Invoke())
            {
                TransientFaultHandlingDbConfiguration.SuspendExecutionStrategy = true;

                using (var newContextTransaction = newContext.Database.BeginTransaction())
                {
                    try
                    {
                        using (var sourceContext = this.sourceContextFactory.Invoke())
                        {
                            var messages = sourceContext.Set<MessageLog>()
                                            .OrderBy(m => m.Id)
                                            .AsEnumerable()
                                            .Select(this.CreateMessage)
                                            .AsCachedAnyEnumerable();



                            this.perfCounter.OnDbConnectionOpenedAndCleansed();
                            this.perfCounter.OnStartingStreamProcessing();

                            this.ProcessMessages(messages);

                            this.perfCounter.OnStreamProcessingFinished();
                            this.perfCounter.OnStartingCommitting();

                            // el borrado colocamos al final por si se este haciendo desde el mismo connection.
                            newContext.Database.ExecuteSqlCommand(@"
                                                DELETE FROM [EventStore].[Events]
                                                DELETE FROM [EventStore].[Snapshots]
                                                DELETE FROM [MessageLog].[Messages]
                                                DBCC CHECKIDENT ('[MessageLog].[Messages]', RESEED, 0)");



                        }

                        rowsAffected = +newContext.SaveChanges();

                        newContextTransaction.Commit();

                        this.perfCounter.OnCommitted(rowsAffected);
                    }
                    catch (Exception)
                    {
                        newContextTransaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        TransientFaultHandlingDbConfiguration.SuspendExecutionStrategy = false;
                    }
                }
            }
        }

        private int GetMessagesCount()
        {
            var sql = new SqlCommandWrapper(config.SourceEventStoreConnectionString);
            return sql.ExecuteReader(@"
                        select count(*) as RwCnt 
                        from MessageLog.Messages 
                        ", r => r.SafeGetInt32(0))
                         .FirstOrDefault();
        }

        private void ProcessMessages(IEnumerable<MessageForDelivery> messages)
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
            if (this.isDuplicateCommand(command))
                return;

            this.commandProcessor.ProcessMessage(command);
            this.ProcessInnerMessages();
        }

        private void ProcessEvent(IEvent @event)
        {
            if (this.isDuplicateEvent(@event))
                return;

            this.eventDispatcher.DispatchMessage(@event, null, string.Empty, string.Empty);
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

        private MessageForDelivery CreateMessage(MessageLog message)
        {
            return new MessageForDelivery(message.Payload);
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
