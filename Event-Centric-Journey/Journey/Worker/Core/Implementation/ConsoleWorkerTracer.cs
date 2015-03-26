using System;

namespace Journey.Worker
{
    public class ConsoleWorkerTracer : IWorkerRoleTracer
    {
        public void Notify(string info)
        {
            Console.WriteLine(new string('-', 80));
            Console.WriteLine(info);
        }
    }
}
