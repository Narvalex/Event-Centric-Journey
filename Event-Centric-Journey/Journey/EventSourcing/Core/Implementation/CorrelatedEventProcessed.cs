namespace Journey.EventSourcing
{
    public class CorrelatedEventProcessed : VersionedEvent
    {
        public string CorrelatedEventTypeName { get; set; }
        public int CorrelatedEventVersion { get; set; }
    }
}
