namespace Infrastructure.CQRS.Worker
{
    public interface IWorkerRoleTracer
    {
        void Notify(string info);
    }
}
