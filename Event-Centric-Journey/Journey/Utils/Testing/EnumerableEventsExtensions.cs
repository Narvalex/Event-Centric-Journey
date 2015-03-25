using Infrastructure.CQRS.EventSourcing;
using Infrastructure.CQRS.Messaging;
using System.Linq;

namespace Infrastructure.CQRS.Utils.Testing
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
