using System;

namespace Journey.Messaging
{
    public abstract class Command
    {
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
