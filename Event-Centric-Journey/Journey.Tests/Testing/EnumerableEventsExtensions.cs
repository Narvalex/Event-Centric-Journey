using Journey.EventSourcing;
using Journey.Messaging;
using System.Linq;

namespace Journey.Tests.Testing
{
    public static class EnumerableEventsExtensions
    {
        public static TEvent SingleEvent<TEvent>(this IEventSourced aggregate)
            where TEvent : IEvent
        {
            return (TEvent)aggregate.Events.Single();
        }
    }
}
