namespace Journey.Worker
{
    public class WorkerRoleStatusUrlProvider : IWorkerRoleStatusUrlProvider
    {
        public WorkerRoleStatusUrlProvider(string statusUrl)
        {
            this.WorkerRoleStatusUrl = statusUrl;
        }

        public string WorkerRoleStatusUrl { get; set; }
    }
}
