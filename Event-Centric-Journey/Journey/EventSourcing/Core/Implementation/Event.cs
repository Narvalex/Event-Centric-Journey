using System;

namespace Journey.EventSourcing
{
    public class TraceableEventSourcedEntity
    {
        public TraceableEventSourcedEntity()
        { }

        public TraceableEventSourcedEntity(ITraceableVersionedEvent @event)
        {
            this.AggregateId = @event.SourceId;
            this.AggregateType = @event.AggregateType;
            this.Version = @event.Version;
            this.TaskCommandId = @event.TaskCommandId;
        }

        public Guid AggregateId { get; set; }
        public string AggregateType { get; set; }
        public int Version { get; set; }
        public Guid TaskCommandId { get; set; }
    }

    public class Event : TraceableEventSourcedEntity
    {
        // Following could is very useful when rebuilding the read model from the event store, 
        // to avoid replaying every possible event in the system
        public string EventType { get; set; }
        public string Payload { get; set; }
        public DateTime CreationDate { get; set; }
        public string CorrelationId { get; set; }

    }
}
