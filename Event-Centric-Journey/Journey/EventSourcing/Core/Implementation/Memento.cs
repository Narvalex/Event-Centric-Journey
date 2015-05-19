
using System.Collections.Generic;

namespace Journey.EventSourcing
{
    public abstract class Memento : IMemento
    {
        public Memento(int version)
        {
            this.Version = version;
        }

        public int Version { get; private set; }
    }

    public abstract class ComplexMemento : Memento
    {
        public ComplexMemento(int version, KeyValuePair<string, int>[] lastProcessedEvents, IVersionedEvent[] earlyReceivedEvents)
            : base(version)
        {
            this.LastProcessedEvents = lastProcessedEvents;
            this.EarlyReceivedEvents = earlyReceivedEvents;
        }

        public KeyValuePair<string, int>[] LastProcessedEvents { get; private set; }
        public IVersionedEvent[] EarlyReceivedEvents { get; private set; }

    }
}
