using System;

namespace Journey.EventSourcing.ReadModeling
{
    public interface IDao
    {
        void WaitEventualConsistencyDelay<T>(Guid commandId) where T : TraceableEventSourcedEntity;
    }
}
