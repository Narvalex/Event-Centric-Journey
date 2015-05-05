using System;

namespace Journey.Messaging
{
    public abstract class Command : ICommand
    {
        /// <summary>
        /// Creates a new instance thats inherits from <see cref="Command"/> and implements <see cref="ICommand"/>.
        /// </summary>
        /// <param name="id">The command identifier.</param>
        /// <remarks>
        /// The Id of a command shoud be always a new Guid, due the use of it for correlation identifiers of events that can 
        /// be raised upon the commanding action.
        /// </remarks>
        public Command(Guid id)
        {
            this.Id = id;
        }

        /// <summary>
        /// The command identifier.
        /// </summary>
        public Guid Id { get; private set; }

        public DateTime CreationDate { get; set; }
    }
}
