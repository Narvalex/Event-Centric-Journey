using System;

namespace Journey.Worker
{
    public interface IWorkerRole : IDisposable
    {
        void Start();

        void Stop();

        IWorkerRoleTracer Tracer { get; }
    }
}
