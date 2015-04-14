namespace Journey.Client
{
    public interface IClientApplicationConfig
    {
        string BusConnectionString { get; }

        string ReadModelConnectionString { get; }

        string CommandBusTableName { get; }

        string EventBusTableName { get; }

        string WorkerRoleStatusUrl { get; }

        int EventualConsistencyCheckRetryPolicy { get; }
    }
}
