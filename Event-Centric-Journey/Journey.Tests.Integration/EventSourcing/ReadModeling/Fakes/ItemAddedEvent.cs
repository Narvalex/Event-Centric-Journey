using Journey.EventSourcing;

namespace Journey.Tests.Integration.EventSourcing.ReadModeling
{
    public class ItemAdded : InternalVersionedEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
