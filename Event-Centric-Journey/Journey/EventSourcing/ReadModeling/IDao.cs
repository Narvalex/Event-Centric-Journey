using System;

namespace Infrastructure.CQRS.EventSourcing.ReadModeling
{
    public interface IDao
    {
        void WaitEventualConsistencyDelay<T>(Guid commandId) where T : TraceableEventSourcedEntity;
    }
}
