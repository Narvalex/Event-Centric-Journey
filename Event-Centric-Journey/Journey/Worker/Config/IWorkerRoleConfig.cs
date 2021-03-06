﻿using Journey.Utils.SystemTime;
using System;

namespace Journey.Worker.Config
{
    public interface IWorkerRoleConfig
    {
        /// <summary>
        /// The event store and bus connection string. Bus and Event Store should be in the 
        /// same db in order to be trasactional
        /// </summary>
        string EventStoreConnectionString { get; }

        string ReadModelConnectionString { get; }

        int NumberOfProcessorsThreads { get; }

        string CommandBusTableName { get; }

        string EventBusTableName { get; }

        TimeSpan BusPollDelay { get; }

        ISystemTime SystemTime { get; }
    }
}
