using Journey.EventSourcing;

namespace Journey.Tests.Integration.ReadModeling
{
    public class ItemView
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
    }
}
