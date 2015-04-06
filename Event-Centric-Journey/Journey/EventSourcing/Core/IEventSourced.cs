using System;
using System.Collections.Generic;

namespace Journey.EventSourcing
{
    /// <summary>
    /// Represents an identifiable entity/document/aggregate/actor that is event sourced.
    /// </summary>
    public interface IEventSourced
    {
        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Gets the collection of news events since the entity was loaded, as a consequence of command handling.
        /// </summary>
        IEnumerable<IVersionedEvent> Events { get; }
    }
}
