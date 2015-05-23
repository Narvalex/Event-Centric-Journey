using System;
using System.Collections.Generic;

namespace Journey.EventSourcing
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
        private readonly List<IVersionedEvent> pendingEvents = new List<IVersionedEvent>();

        private readonly Guid id;
        //private int version = -1;
        protected int version = 0;

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
        public IEnumerable<IVersionedEvent> Events
        {
            get { return this.pendingEvents; }
        }

        protected void LoadFrom(IEnumerable<IVersionedEvent> pastEvents)
        {
            foreach (var @event in pastEvents)
            {
                ((dynamic)this).Rehydrate((dynamic)@event);
                this.version = @event.Version;
            }
        }

        protected void Update(InternalVersionedEvent @event)
        {
            @event.SourceId = this.Id;
            @event.Version = this.version + 1;
            @event.SourceType = this.GetType().Name;
            ((dynamic)this).Rehydrate((dynamic)@event);
            this.version = @event.Version;
            this.pendingEvents.Add(@event);
        }
    }
}
