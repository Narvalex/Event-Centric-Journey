using System.Collections.Generic;
namespace Journey.Worker
{
    public interface IWorkerRoleTracer
    {
        void Trace(string info);

        void Notify(string info);

        void Notify(IEnumerable<string> notifications);
    }
}
