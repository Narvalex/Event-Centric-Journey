using System;

namespace Infrastructure.CQRS
{
    public interface IAggregateRoot
    {
        /// <summary>
        /// Gets the entity identifier.
        /// </summary>
        Guid Id { get; }
    }
}
