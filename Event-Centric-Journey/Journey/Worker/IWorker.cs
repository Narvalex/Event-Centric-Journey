using System;

namespace Journey.Worker
{
    public interface IWorker : IDisposable
    {
        void Start();

        void Stop();
    }
}
