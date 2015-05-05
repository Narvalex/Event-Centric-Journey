using System;

namespace Journey.Messaging
{
    public interface ICommand : IMessage
    {
        /// <summary>
        /// Gets the command identifier.
        /// </summary>
        Guid Id { get; }
    }
}
