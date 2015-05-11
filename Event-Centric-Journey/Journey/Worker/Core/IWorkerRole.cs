using System;

namespace Journey.Worker
{
    public interface IWorkerRole : IDisposable
    {
        void Start();

        void Stop();

        ITracer Tracer { get; }
    }
}
