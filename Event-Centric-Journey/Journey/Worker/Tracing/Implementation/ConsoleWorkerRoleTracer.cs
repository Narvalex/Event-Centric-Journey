using System;

namespace Journey.Worker
{
    public class ConsoleWorkerRoleTracer : IWorkerRoleTracer
    {
        public void Notify(string info)
        {
            Console.WriteLine(new string('-', 80));
            Console.WriteLine(info);
        }
    }
}
