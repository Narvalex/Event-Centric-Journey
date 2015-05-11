using System.Collections.Generic;
namespace Journey.Worker
{
    public interface ITracer
    {
        void TraceAsync(string info);

        void Notify(string info);

        void Notify(IEnumerable<string> notifications);
    }
}
