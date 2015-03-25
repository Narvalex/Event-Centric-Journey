using System;

namespace Journey
{
    public interface IAggregateRoot
    {
        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        Guid Id { get; }
    }
}
