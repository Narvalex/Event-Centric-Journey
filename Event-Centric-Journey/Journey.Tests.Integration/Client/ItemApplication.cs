using Journey.Client;
using Journey.EventSourcing.ReadModeling;
using Journey.Messaging;
using Journey.Utils.Guids;
using System;

namespace Journey.Tests.Integration.Client
{
    public class ItemApplication
    {
        private readonly IClientApplication app;
        private readonly IClientApplication buggyApp;

        public ItemApplication(ICommandBus commandBus, string workerRoleStatusUrl, Func<ReadModelDbContext> readModelContextFactory)
        {
            this.app = new ClientApplication(commandBus, workerRoleStatusUrl, readModelContextFactory, 10);
            this.buggyApp = new ClientApplication(new BuggyCommandBus(), workerRoleStatusUrl, readModelContextFactory, 1);
        }

        public void AddItem(string name)
        {
            var command = new AddItem(SequentialGuid.GenerateNewGuid());
            this.app.Send(command);
        }

        public void AddItemBuggyProcess(string name)
        {
            var command = new AddItem(SequentialGuid.GenerateNewGuid());
            this.buggyApp.Send(command);
        }

        private class BuggyCommandBus : ICommandBus
        {
            public void Send(Envelope<ICommand> command)
            {
            }

            public void Send(System.Collections.Generic.IEnumerable<Envelope<ICommand>> commands)
            {
            }

            public void Send(System.Collections.Generic.IEnumerable<Envelope<ICommand>> commands, System.Data.Entity.DbContext context)
            {
            }
        }
    }

    public class AddItem : ExternalCommand
    {
        public AddItem(Guid id)
            : base(id)
        { }
    }
}
