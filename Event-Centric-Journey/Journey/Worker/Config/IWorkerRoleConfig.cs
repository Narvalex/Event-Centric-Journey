using System;

namespace Journey.Worker.Config
{
    public interface IWorkerRoleConfig
    {
        string BusConnectionString { get; }

        string EventStoreConnectionString { get; }

        string MessageLogConnectionString { get; }

        int NumberOfProcessorsThreads { get; }

        TimeSpan BusPollDelay { get; }
    }
}
