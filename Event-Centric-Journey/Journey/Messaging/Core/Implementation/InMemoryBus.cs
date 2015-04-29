using System.Collections.Generic;
using System.Linq;

namespace Journey.Messaging
{
    public class InMemoryBus : IInMemoryBus
    {
        private readonly Queue<ICommand> commands;
        private readonly Queue<IEvent> events;

        public InMemoryBus()
        {
            this.commands = new Queue<ICommand>();
            this.events = new Queue<IEvent>();
        }

        public void Send(IEnumerable<ICommand> commands)
        {
            foreach (var command in commands)
                this.commands.Enqueue(command);
        }

        public void Publish(IEnumerable<IEvent> events)
        {
            foreach (var e in events)
                this.events.Enqueue(e);
        }

        public bool HasNewCommands
        {
            get { return this.commands.Any(); }
        }

        public bool HasNewEvents
        {
            get { return this.events.Any(); }
        }

        public IEnumerable<ICommand> GetCommands()
        {
            for (int i = 0; i < this.commands.Count; i++)
                yield return this.commands.Dequeue();
        }

        public IEnumerable<IEvent> GetEvents()
        {
            for (int i = 0; i < this.events.Count; i++)
                yield return this.events.Dequeue();
        }
    }
}
