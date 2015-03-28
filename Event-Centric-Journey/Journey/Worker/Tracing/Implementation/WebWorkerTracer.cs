using Journey.Worker.Tracing;
using System;
using System.Threading.Tasks;

namespace Journey.Worker
{
    public class WebWorkerTracer : IWebTracer
    {
        public readonly Action<string> notify;

        public WebWorkerTracer(Action<string> notify)
        {
            this.notify = notify;
        }

        public void Notify(string info)
        {
            Task.Factory.StartNew(() => this.notify(info), TaskCreationOptions.PreferFairness);
        }
    }
}
