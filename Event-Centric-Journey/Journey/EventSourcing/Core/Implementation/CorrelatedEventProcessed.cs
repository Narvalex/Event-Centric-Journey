namespace Journey.EventSourcing
{
    public class CorrelatedEventProcessed : InternalVersionedEvent
    {
        public string CorrelatedEventTypeName { get; set; }
        public int CorrelatedEventVersion { get; set; }
    }
}
