namespace Journey.EventSourcing
{
    public class EarlyEventReceived : InternalVersionedEvent
    {
        public object Event { get; set; }
    }
}
