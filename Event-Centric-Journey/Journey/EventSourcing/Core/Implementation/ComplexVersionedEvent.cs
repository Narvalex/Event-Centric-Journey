
namespace Journey.EventSourcing
{
    public abstract class ComplexVersionedEvent : VersionedEvent
    {
        public int LastSourceEventVersion { get; set; }
        public string SourceEventType { get; set; }
    }
}
