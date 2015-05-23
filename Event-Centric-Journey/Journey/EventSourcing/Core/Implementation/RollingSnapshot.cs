using System;
using System.ComponentModel.DataAnnotations;

namespace Journey.EventSourcing
{
    public class RollingSnapshot
    {
        [StringLength(300)]
        public string PartitionKey { get; set; }
        public string Memento { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }
}
