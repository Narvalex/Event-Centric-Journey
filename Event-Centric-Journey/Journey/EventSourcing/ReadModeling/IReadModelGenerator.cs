using System;

namespace Journey.EventSourcing.ReadModeling
{
    public interface IReadModelGenerator<T> where T : ReadModelDbContext
    {
        void Project(IVersionedEvent e, Action<T> unitOfWork, bool isLiveProjection = true);
    }
}
