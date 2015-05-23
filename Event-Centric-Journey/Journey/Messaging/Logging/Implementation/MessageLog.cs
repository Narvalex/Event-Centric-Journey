
using System.ComponentModel.DataAnnotations;
namespace Journey.Messaging.Logging
{
    public class MessageLog
    {
        public long Id { get; set; }

        [StringLength(50)]
        public string Kind { get; set; }

        [StringLength(50)]
        public string SourceId { get; set; }

        [StringLength(50)]
        public string Version { get; set; }

        [StringLength(255)]
        public string AssemblyName { get; set; }

        [StringLength(255)]
        public string Namespace { get; set; }

        [StringLength(255)]
        public string FullName { get; set; }

        [StringLength(255)]
        public string TypeName { get; set; }

        [StringLength(255)]
        public string SourceType { get; set; }

        [StringLength(50)]
        public string CreationDate { get; set; }

        [StringLength(50)]
        public string LastUpdateTime { get; set; }

        public string Payload { get; set; }

        [StringLength(50)]
        public string Origin { get; set; }
    }
}
