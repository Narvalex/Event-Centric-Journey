using System.Collections.Generic;

namespace Infrastructure.CQRS.Messaging.Logging.Metadata
{
    /// <summary>
    /// Extracts metadata about a payload so that it's placed in the message envelope.
    /// </summary>
    public interface IMetadataProvider
    {
        /// <summary>
        /// Gets metadata associated with the payload.
        /// </summary>
        IDictionary<string, string> GetMetadata(object payload);
    }
}
