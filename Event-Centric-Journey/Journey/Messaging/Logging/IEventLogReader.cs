using System.Collections.Generic;

namespace Journey.Messaging.Logging
{
    /// <summary>
    /// Exposes the message log for all events that the system processed.
    /// </summary>
    public interface IEventLogReader
    {
        IEnumerable<IEvent> Query(EventLogQueryCriteria criteria);
    }
}
