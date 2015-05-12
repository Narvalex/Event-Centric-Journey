using System;

namespace Journey.Worker
{
    public class ConsoleTracer : ITracer
    {
        public void TraceAsync(string info)
        {
            Console.WriteLine(new string('-', 80));
            Console.WriteLine(info);
        }


        public void Notify(string info)
        {
            throw new NotImplementedException();
        }

        public void Notify(System.Collections.Generic.IEnumerable<string> notifications)
        {
            throw new NotImplementedException();
        }


        public void DisableTracing()
        {
            throw new NotImplementedException();
        }

        public void EnableTracing()
        {
            throw new NotImplementedException();
        }
    }
}
