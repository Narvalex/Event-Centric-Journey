using System;

namespace Journey.EventSourcing
{
    public class RollingSnapshot
    {
        public string PartitionKey { get; set; }
        public string Memento { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }
}
