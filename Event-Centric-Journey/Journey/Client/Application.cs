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
    public class Application : IApplication
    {
        private readonly ICommandBus commandBus;
        private readonly Func<ICommand, Task> sendCommandAsync;
        private readonly Func<string, Task> pingTheWorkerRole;
        private readonly Func<Guid, Task> waitEventualConsistencyDelay;
        private readonly Func<ReadModelDbContext> readModelContextFactory;
        private readonly int eventualConsistencyCheckRetryPolicy;

        public Application(ICommandBus commandBus, string workerRoleStatusUrl, Func<ReadModelDbContext> readModelContextFactory, int eventualConsistencyCheckRetryPolicy)
        {
            this.eventualConsistencyCheckRetryPolicy = eventualConsistencyCheckRetryPolicy;
            this.commandBus = commandBus;
            this.readModelContextFactory = readModelContextFactory;

            this.sendCommandAsync = async (command) =>
            {
                await Task.Factory.StartNew(() =>
                {
                    this.commandBus.Send(command);
                });
            };

            this.pingTheWorkerRole = async (commandTypeName) =>
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

            this.waitEventualConsistencyDelay = async (correlationId) =>
            {
                await Task.Factory.StartNew(() =>
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

                            ++retries;
                            Thread.Sleep(100 * retries);
                        }
                    }

                    if (isConsistent == false)
                        throw new TimeoutException("No se pudo verificar la consistencia eventual. Espere unos minutos más");
                });
            };
        }

        /// <summary>
        /// Sends a command to the bus, pings the worker role 
        /// and polls the read model to check the success or failure
        /// of the process, asynchronously.
        /// </summary>
        public void Send(ICommand command)
        {
            var tasks = new HashSet<Task>();

            tasks.Add(this.pingTheWorkerRole(command.GetType().Name));
            tasks.Add(this.sendCommandAsync(command));
            tasks.Add(this.waitEventualConsistencyDelay(command.Id));

            Task.WaitAll(tasks.ToArray());
        }

        private ReadModelDbContext CreateReadOnlyDbContext()
        {
            var context = this.readModelContextFactory.Invoke();
            context.Configuration.AutoDetectChangesEnabled = false;
            return context;
        }
    }
}
