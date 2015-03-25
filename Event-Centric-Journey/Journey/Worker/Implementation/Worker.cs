using Journey.Messaging.Processing;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Journey.Worker
{
    public class Worker : IWorker
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly IUnityContainer container;
        private readonly List<IMessageProcessor> processors;

        public Worker(IUnityContainer container)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.container = container;
            this.processors = this.container.ResolveAll<IMessageProcessor>().ToList();
        }

        public void Start()
        {
            this.processors.ForEach(p => p.Start());
        }

        public void Stop()
        {
            this.cancellationTokenSource.Cancel();

            this.processors.ForEach(p => p.Stop());
        }

        public void Dispose()
        {
            this.container.Dispose();
            this.cancellationTokenSource.Dispose();
        }
    }
}
