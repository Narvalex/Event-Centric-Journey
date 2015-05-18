using Journey.Utils;
using System;
using System.Collections.Generic;

namespace Journey.EventSourcing
{
    public abstract class ComplexEventProcessor : EventSourced
    {
        private readonly IDictionary<string, int> lastProcessedEvents;

        public ComplexEventProcessor(Guid id)
            : base(id)
        {
            this.lastProcessedEvents = new Dictionary<string, int>();
        }

        public bool TryProcessWithGuaranteedIdempotency(IVersionedEvent @event)
        {
            var eventType = @event.GetType().FullName;

            var lastProcessedEventVersion = this.lastProcessedEvents.TryGetValue(eventType);

            if (lastProcessedEventVersion <= @event.Version)
            {
                // el evento ya fue procesado.
                return false;
            }
            else if (lastProcessedEventVersion == @event.Version - 1)
            {
                ((dynamic)this).Process((dynamic)@event);
                return true;
            }

            // El caso cuando el evento es muy nuevo todavia y falta otro anterior.
        }

        protected void UpdateFromSourceEvent(ComplexVersionedEvent sourceEvent, ComplexVersionedEvent @event)
        {
            @event.LastSourceEventVersion = sourceEvent.Version;

            base.Update(@event);
        }
    }
}
