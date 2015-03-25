using System;

namespace Journey.EventSourcing
{
    public interface IEventStore<T> where T : IEventSourced
    {
        /// <summary>
        /// Tries to retrieve the event sourced entity.
        /// </summary>
        /// <param name="id">The id of the entity</param>
        /// <returns>The hydrated entity, or null if it does not exist.</returns>
        T Find(Guid id);

        /// <summary>
        /// Retrieves the event sourced entity.
        /// </summary>
        /// <param name="id">The id of the entity</param>
        /// <returns>The hydrated entity</returns>
        /// <exception cref="EntityNotFoundException">If the entity is not found.</exception>
        T Get(Guid id);

        /// <summary>
        /// Saves the event sourced entity in a distributed transaction that wraps 
        /// the database update and the message publishing.
        /// </summary>
        /// <param name="eventSourced">The entity.</param>
        /// <param name="correlationId">A correlation id to use when publishing events. It could be either a command id or an event sourced aggregate id that raised an event.</param>
        void Save(T eventSourced, Guid correlationId);
    }
}
