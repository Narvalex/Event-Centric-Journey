using Journey.EventSourcing;

namespace Journey.Tests.Integration.EventSourcing.ReadModeling
{
    public class ItemAdded : VersionedEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
