using Journey.Serialization;
using Journey.Worker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Journey.Messaging.Processing
{
    /// <summary>
    /// Processes incoming commands from the bus and routes them to the appropriate handlers.
    /// </summary>
    public class CommandProcessor : MessageProcessor, ICommandHandlerRegistry, ICommandProcessor
    {
        private Dictionary<Type, ICommandHandler> handlers = new Dictionary<Type, ICommandHandler>();
        private readonly ICommandBusTransientFaultDetector faultDetector;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandProcessor"/> class.
        /// </summary>
        /// <param name="receiver">The receiver to use. If the receiver is <see cref="IDisposable"/>, it will be
        ///  disposed.</param>
        /// <param name="serializer">The serializer to use for the message body.</param>
        public CommandProcessor(IMessageReceiver receiver, ITextSerializer serializer, ITracer tracer, ICommandBusTransientFaultDetector faultDetector)
            : base(receiver, serializer, tracer)
        {
            this.faultDetector = faultDetector;
        }

        /// <summary>
        /// Registers the specified command handler.
        /// </summary>
        public void Register(ICommandHandler commandHandler)
        {
            var genericHandler = typeof(ICommandHandler<>);
            var supportedCommandTypes =
                commandHandler
                .GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericHandler)
                .Select(i => i.GetGenericArguments()[0])
                .ToList();

            if (handlers.Keys.Any(registeredType => supportedCommandTypes.Contains(registeredType)))
                throw new ArgumentException("The command handled by the received handler already has a registered handler.");

            // Register this handler for each of the handled types.
            foreach (var commandType in supportedCommandTypes)
                this.handlers.Add(commandType, commandHandler);
        }

        /// <summary>
        /// Processes the message by calling the registered handler.
        /// </summary>        
        protected override void ProcessMessage(object payload, string correlationId)
        {
            ICommandHandler handler = null;

            // Esto es util por si el comando no se borro todavia, pero se proceso en el event store
            // If command was already processed, then no op in processing, but maybe we can log it
            if (!this.faultDetector.CommandWasAlreadyProcessed(payload))
            {
                var commandType = payload.GetType();
                if (this.handlers.TryGetValue(commandType, out handler))
                    // Actualy handling the message
                    this.HandleMessage(payload, handler);
            }


            // There can be a generic logging/tracing/auditing handlers
            // El message log verifica que no existe el comando en el log
            if (this.handlers.TryGetValue(typeof(ICommand), out handler))
            {
                this.HandleMessage(payload, handler);
            }
        }

        private void HandleMessage(object payload, ICommandHandler handler)
        {
            // Litle retry policy
            var attempts = 0;
            while (true)
            {
                try
                {
                    ((dynamic)handler).Handle((dynamic)payload);
                    break;
                }
                catch (Exception e)
                {
                    ++attempts;
                    if (attempts >= 3)
                        throw;

                    this.tracer.TraceAsync(new string('-', 80));
                    this.tracer.TraceAsync(string.Format(
                        "Handle command attempt number {0}. An exception happened while processing message through handler: {1}\r\n{2}", attempts, handler.GetType().Name, e));
                    this.tracer.TraceAsync(new string('-', 80)); 
                }
            }

            
            base.tracer.TraceAsync("Command handled by " + handler.GetType().Name);
        }

        public void ProcessMessage(object payload)
        {
            this.ProcessMessage(payload, string.Empty);
        }
    }
}
