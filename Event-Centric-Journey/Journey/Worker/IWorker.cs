using System;

namespace Infrastructure.CQRS.Worker
{
    public interface IWorker : IDisposable
    {
        void Start();

        void Stop();
    }
}
