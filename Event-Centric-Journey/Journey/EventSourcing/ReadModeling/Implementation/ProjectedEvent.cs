using System;

namespace Journey.EventSourcing.ReadModeling
{
    public class ProjectedEvent : IProcessedEvent
    {
        public Guid AggregateId { get; set; }

        public string AggregateType { get; set; }

        public int Version { get; set; }

        public string EventType { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
