using System;

namespace Infrastructure.CQRS.Messaging
{
    public interface ICommand
    {
        /// <summary>
        /// Gets the command identifier.
        /// </summary>
        Guid Id { get; }
    }
}
