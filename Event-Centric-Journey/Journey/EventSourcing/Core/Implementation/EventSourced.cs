using Infrastructure.CQRS.Messaging;
using System;
using System.Collections.Generic;

namespace Infrastructure.CQRS.EventSourcing
{
    /// <summary>
    /// Base class for event sourced entities that implements <see cref="IEventSourced"/>. 
    /// </summary>
    /// <remarks>
    /// <see cref="IEventSourced"/> entities do not require the use of <see cref="EventSourced"/>, but this class contains some common 
    /// useful functionality related to versions and rehydration from past events.
    /// </remarks>
    public abstract class EventSourced : IEventSourced
    {
        private readonly Dictionary<Type, Action<ITraceableVersionedEvent>> rehydrators = new Dictionary<Type, Action<ITraceableVersionedEvent>>();
        private readonly List<ITraceableVersionedEvent> pendingEvents = new List<ITraceableVersionedEvent>();

        private readonly Guid id;
        //private int version = -1;
        private int version = 0;

        protected EventSourced(Guid id)
        {
            this.id = id;
        }

        public Guid Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
        /// </summary>
        public int Version
        {
            get { return this.version; }
            protected set { this.version = value; }
        }

        /// <summary>
        /// Gets the collection of new events since the entity was loaded, as a consequence of command handling.
        /// </summary>
        public IEnumerable<ITraceableVersionedEvent> Events
        {
            get { return this.pendingEvents; }
        }

        /// <summary>
        /// Configures a handler for an event. [Handles}
        /// </summary>
        protected void RehydratesFrom<TEvent>(Action<TEvent> handler)
            where TEvent : IEvent
        {
            this.rehydrators.Add(typeof(TEvent), @event => handler((TEvent)@event));
        }

        protected void LoadFrom(IEnumerable<ITraceableVersionedEvent> pastEvents)
        {
            foreach (var e in pastEvents)
            {
                this.rehydrators[e.GetType()].Invoke(e);
                this.version = e.Version;
            }
        }

        protected void Update(TraceableVersionedEvent e)
        {
            e.SourceId = this.Id;
            e.Version = this.version + 1;
            e.AggregateType = this.GetType().Name;
            this.rehydrators[e.GetType()].Invoke(e);
            this.version = e.Version;
            this.pendingEvents.Add(e);
        }
    }
}
