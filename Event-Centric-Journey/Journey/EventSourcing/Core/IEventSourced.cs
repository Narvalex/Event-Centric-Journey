using System.Collections.Generic;

namespace Journey.EventSourcing
{
    /// <summary>
    /// Represents an identifiable entity that is event sourced.
    /// </summary>
    public interface IEventSourced : IAggregateRoot
    {
        /// <summary>
        /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Gets the collection of news events since the entity was loaded, as a consequence of command handling.
        /// </summary>
        IEnumerable<ITraceableVersionedEvent> Events { get; }
    }
}
