using Journey.EventSourcing.ReadModeling;
using Journey.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Journey.Client
{
    public class ClientApplication : IClientApplication
    {
        private readonly ICommandBus commandBus;
        private readonly Func<string, Task> pingWorkerRoleAsync;
        private readonly Action<Guid> waitEventualConsistencyDelay;
        private readonly Func<ReadModelDbContext> readModelContextFactory;
        private readonly int eventualConsistencyCheckRetryPolicy;

        public ClientApplication(ICommandBus commandBus, string workerRoleStatusUrl, Func<ReadModelDbContext> readModelContextFactory, int eventualConsistencyCheckRetryPolicy)
        {
            this.eventualConsistencyCheckRetryPolicy = eventualConsistencyCheckRetryPolicy;
            this.commandBus = commandBus;
            this.readModelContextFactory = readModelContextFactory;

            this.pingWorkerRoleAsync = async (commandTypeName) =>
            {
                await Task.Factory.StartNew(async () =>
                {
                    using (var httpClient = new HttpClient())
                    {
                        var workerResponse = string.Empty;
                        var retries = 0;
                        while (workerResponse == string.Empty && retries <= 20)
                        {
                            if (retries > 0)
                                Thread.Sleep(100 * retries);

                            workerResponse = await httpClient.GetStringAsync(
                                string.Format("{0}/?requester=COMMAND_{1}", workerRoleStatusUrl, commandTypeName));
                            ++retries;
                        }
                    }
                });
            };

            this.waitEventualConsistencyDelay = (correlationId) =>
            {
                    var retries = 0;
                    var isConsistent = false;

                    while (retries < 20)
                    {
                        using (var context = this.CreateReadOnlyDbContext())
                        {
                            isConsistent = context
                                            .ProcessedEvents
                                            .Where(e => e.CorrelationId == correlationId).Any();

                            if (isConsistent)
                                break;

                           
                        }

                        ++retries;
                        Thread.Sleep(100 * retries);
                    }

                    if (isConsistent == false)
                        throw new TimeoutException("No se pudo verificar la consistencia eventual. Espere unos minutos más");
            };
        }

        /// <summary>
        /// Sends a command to the bus, pings the worker role 
        /// and polls the read model to check the success or failure
        /// of the process, asynchronously.
        /// </summary>
        public void Send(ICommand command)
        {
            this.pingWorkerRoleAsync(command.GetType().Name);
            this.commandBus.Send(command);
            this.waitEventualConsistencyDelay(command.Id);

        }

        private ReadModelDbContext CreateReadOnlyDbContext()
        {
            var context = this.readModelContextFactory.Invoke();
            context.Configuration.AutoDetectChangesEnabled = false;
            return context;
        }
    }
}
