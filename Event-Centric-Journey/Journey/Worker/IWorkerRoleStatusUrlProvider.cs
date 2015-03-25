namespace Infrastructure.CQRS.Worker
{
    public interface IWorkerRoleStatusUrlProvider
    {
        string WorkerRoleStatusUrl { get; }
    }
}
