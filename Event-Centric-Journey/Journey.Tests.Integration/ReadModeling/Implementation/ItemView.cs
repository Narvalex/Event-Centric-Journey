using Journey.EventSourcing;

namespace Journey.Tests.Integration.ReadModeling.Implementation
{
    public class ItemView : TraceableEventSourcedEntity
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
    }
}
