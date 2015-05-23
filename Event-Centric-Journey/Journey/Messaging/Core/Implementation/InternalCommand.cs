using System;

namespace Journey.Messaging
{
    public abstract class InternalCommand : Command, ICommand
    {
        /// <summary>
        /// Creates a new instance thats inherits from <see cref="ExternalCommand"/> and implements <see cref="ICommand"/>.
        /// </summary>
        /// <param name="id">The command identifier.</param>
        /// <remarks>
        /// The Id of a command shoud be always a new Guid, due the use of it for correlation identifiers of events that can 
        /// be raised upon the commanding action.
        /// </remarks>
        public InternalCommand(Guid id)
            : base(id)
        {
            this.IsExternal = false;
        }

        public bool IsExternal { get; private set; }
    }
}
