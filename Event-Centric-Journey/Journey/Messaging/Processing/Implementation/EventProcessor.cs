using Journey.Serialization;
using Journey.Worker;

namespace Journey.Messaging.Processing
{
    /// <summary>
    /// Processes incoming events from the bus and routes them to the appropriate 
    /// handlers.
    /// </summary>
    public class EventProcessor : MessageProcessor, IEventHandlerRegistry
    {
        private readonly IEventDispatcher messageDispatcher;

        public EventProcessor(IMessageReceiver receiver, ITextSerializer serializer, ITracer tracer)
            : base(receiver, serializer, tracer)
        {
            this.messageDispatcher = new AsynchronousEventDispatcher(base.tracer);
        }

        public void Register(IEventHandler eventHandler)
        {
            this.messageDispatcher.Register(eventHandler);
        }

        protected override void ProcessMessage(object payload, string correlationId)
        {
            var @event = (IEvent)payload;
            this.messageDispatcher.DispatchMessage(@event, null, correlationId, "");
        }
    }
}
