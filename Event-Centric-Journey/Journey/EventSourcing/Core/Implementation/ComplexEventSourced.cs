using Journey.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Journey.EventSourcing
{
    public abstract class ComplexEventSourced : EventSourced,
        IRehydratesFrom<EarlyEventReceived<IVersionedEvent>>,
        IRehydratesFrom<CorrelatedEventProcessed<IVersionedEvent>>
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
            var eventTypeName = ((object)@event).GetType().FullName;

            var lastProcessedEventVersion = this.lastProcessedEvents.TryGetValue(eventTypeName);

            if (@event.Version <= lastProcessedEventVersion)
            {
                // el evento ya fue procesado.
                return false;
            }
            else if (lastProcessedEventVersion == @event.Version - 1)
            {
                // el evento vino en el orden correcto
                ((dynamic)this).Process((dynamic)@event);
                base.Update(new CorrelatedEventProcessed<IVersionedEvent>
                    {
                        Event = @event
                    });

                this.ProcessEarlyEventsIfApplicable();
            }
            else
            {
                // verificando que no se agregue el mismo evento con la misma version varias veces
                if (this.earlyReceivedEvents
                    .Where(e => e.Version == @event.Version
                           && ((object)e).GetType().FullName == eventTypeName)
                    .Any())
                    return false;

                // el evento vino prematuramente, se almacena para procesarlo en el orden correcto
                this.Update(new EarlyEventReceived<IVersionedEvent>
                {
                    Event = @event
                });
            }
            // El caso cuando el evento es muy nuevo todavia y falta otro anterior.
            return true;
        }

        private void ProcessEarlyEventsIfApplicable()
        {
            // Después de cargar todo comprobamos que no exista un evento que no haya sido procesado.
            var eventsOnTime = new List<IVersionedEvent>();
            if (this.earlyReceivedEvents.Count > 0 && this.lastProcessedEvents.Count > 0)
            {
                // Recorremos todos los eventos que se recibieron prematuramente
                foreach (var early in this.earlyReceivedEvents)
                {
                    // obtenemos el nombre del evento prematuro
                    var earlyEventName = ((object)early).GetType().FullName;

                    // verificamos si esta en la lista de eventos procesados un evento recibido prematuramente
                    if (this.lastProcessedEvents.ContainsKey(earlyEventName))
                    {
                        var lastProcessed = lastProcessedEvents[earlyEventName];
                        if (early.Version - 1 == lastProcessed)
                            eventsOnTime.Add(early);
                    }
                }

                foreach (var onTime in eventsOnTime)
                    this.TryProcessWithGuaranteedIdempotency(onTime);
                // se va a marcar aqui si si esta en la lista o no.
            }
        }

        public void Rehydrate(EarlyEventReceived<IVersionedEvent> e)
        {
            this.earlyReceivedEvents.Add(e.Event);
        }

        public void Rehydrate(CorrelatedEventProcessed<IVersionedEvent> e)
        {
            var correlated = e.Event;
            var eventTypeName = ((object)correlated).GetType().FullName;

            // Marcamos como evento procesado en la lista de eventos procesados
            if (this.lastProcessedEvents.ContainsKey(eventTypeName))
                this.lastProcessedEvents[eventTypeName] = correlated.Version;
            else
                this.lastProcessedEvents.Add(eventTypeName, correlated.Version);

            // Verificar aqui si esta en la lista o no.
            var earlyEvent = this.earlyReceivedEvents
                .Where(x => ((object)x).GetType().FullName == eventTypeName
                                && x.Version == correlated.Version)
                .FirstOrDefault();

            if (earlyEvent != null)
                this.earlyReceivedEvents.Remove(earlyEvent);
        }
    }

    public class EarlyEventReceived<T> : VersionedEvent where T : IVersionedEvent
    {
        public T Event { get; set; }
    }

    public class CorrelatedEventProcessed<T> : VersionedEvent where T : IVersionedEvent
    {
        public T Event { get; set; }
    }
}
