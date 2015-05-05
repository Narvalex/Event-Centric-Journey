using System;

namespace Journey.EventSourcing.ReadModeling
{
    public class ProjectedEvent : IProcessedEvent
    {
        public Guid SourceId { get; set; }

        public string SourceType { get; set; }

        public int Version { get; set; }

        public string EventType { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
