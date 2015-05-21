namespace Journey.EventSourcing
{
    public class EarlyEventReceived : VersionedEvent
    {
        public object Event { get; set; }
    }
}
