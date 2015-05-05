using System;

namespace Journey.EventSourcing
{
    public class Event
    {
        // Following could is very useful when rebuilding the read model from the event store, 
        // to avoid replaying every possible event in the system
        public string EventType { get; set; }
        public string Payload { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid AggregateId { get; set; }
        public string AggregateType { get; set; }
        public int Version { get; set; }
    }
}
