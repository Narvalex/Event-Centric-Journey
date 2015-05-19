using Journey.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Journey.EventSourcing
{
    public abstract class ComplexEventSourced : EventSourced,
        IRehydratesFrom<EarlyEventReceived<IVersionedEvent>>,
        IRehydratesFrom<EarlyEventProcessed<IVersionedEvent>>
    {
        private readonly IDictionary<string, int> lastProcessedEvents;
        private readonly IList<IVersionedEvent> earlyReceivedEvents;

        public ComplexEventSourced(Guid id)
            : base(id)
        {
            this.lastProcessedEvents = new Dictionary<string, int>();
            this.earlyReceivedEvents = new List<IVersionedEvent>();
        }

        public bool TryProcessWithGuaranteedIdempotency(IVersionedEvent @event)
        {
            var eventType = @event.GetType().FullName;

            var lastProcessedEventVersion = this.lastProcessedEvents.TryGetValue(eventType);

            if (@event.Version <= lastProcessedEventVersion)
            {
                // el evento ya fue procesado.
                return false;
            }
            else if (lastProcessedEventVersion == @event.Version - 1)
            {
                // el evento vino en el orden correcto
                ((dynamic)this).Process((dynamic)@event);
            }
            else
            {
                // el evento vino prematuramente, se almacena para procesarlo en el orden correcto
                @event.CorrelatedSourceVersion = @event.Version;
                @event.CorrelatedEventType = eventType;

                // verificando que no se agregue el mismo evento con la misma version varias veces
                if (this.earlyReceivedEvents
                    .Where(e => e.CorrelatedSourceVersion == @event.CorrelatedSourceVersion
                                && e.CorrelatedEventType == @event.CorrelatedEventType)
                    .Any())
                    return false;


                var early = new EarlyEventReceived<IVersionedEvent>();
                early.Event = @event;
                this.Update(early);
            }

            // Después de cargar todo comprobamos que no exista un evento que no haya sido procesado.
            var eventsOnTime = new List<IVersionedEvent>();
            if (this.earlyReceivedEvents.Count > 0 && this.lastProcessedEvents.Count > 0)
            {
                foreach (var early in this.earlyReceivedEvents)
                {
                    if (this.lastProcessedEvents.ContainsKey(early.CorrelatedEventType))
                    {
                        var lastVersion = lastProcessedEvents[early.CorrelatedEventType];
                        if (early.CorrelatedSourceVersion - 1 == lastVersion)
                            eventsOnTime.Add(early);
                    }
                }

                foreach (var onTime in eventsOnTime)
                {
                    this.TryProcessWithGuaranteedIdempotency(onTime);
                    var processedEvent = new EarlyEventProcessed<IVersionedEvent>();
                    processedEvent.Event = onTime;
                    this.Update(processedEvent);
                }
            }

            // El caso cuando el evento es muy nuevo todavia y falta otro anterior.
            return true;
        }

        protected override void Update(VersionedEvent @event)
        {
            base.Update(@event);

            this.UpdateLastProcessedEventsIndicator(@event);
        }

        private void UpdateLastProcessedEventsIndicator(IVersionedEvent @event)
        {
            var eventType = string.IsNullOrEmpty(@event.CorrelatedEventType) ? @event.GetType().FullName : @event.CorrelatedEventType;
            if (this.lastProcessedEvents.ContainsKey(@event.GetType().FullName))
                this.lastProcessedEvents[@event.CorrelatedEventType] = @event.CorrelatedSourceVersion;
            else
                this.lastProcessedEvents.Add(@event.CorrelatedEventType, @event.CorrelatedSourceVersion);
        }

        protected override void LoadFrom(IEnumerable<IVersionedEvent> pastEvents)
        {
            foreach (var @event in pastEvents)
            {
                ((dynamic)this).Rehydrate((dynamic)@event);
                this.version = @event.Version;

                this.UpdateLastProcessedEventsIndicator(@event);
            }
        }

        public void Rehydrate(EarlyEventReceived<IVersionedEvent> e)
        {
            this.earlyReceivedEvents.Add(e.Event);
        }

        public void Rehydrate(EarlyEventProcessed<IVersionedEvent> e)
        {
            this.earlyReceivedEvents.Remove(e.Event);
        }
    }

    public class EarlyEventReceived<T> : VersionedEvent where T : IVersionedEvent
    {
        public T Event { get; set; }
    }

    public class EarlyEventProcessed<T> : VersionedEvent where T : IVersionedEvent
    {
        public T Event { get; set; }
    }
}
