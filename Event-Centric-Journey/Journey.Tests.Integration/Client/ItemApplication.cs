using Journey.Client;
using Journey.EventSourcing.ReadModeling;
using Journey.Messaging;
using Journey.Utils.Guids;
using System;

namespace Journey.Tests.Integration.Client
{
    public class ItemApplication
    {
        private readonly IApplication app;
        private readonly IApplication buggyApp;

        public ItemApplication(ICommandBus commandBus, string workerRoleStatusUrl, Func<ReadModelDbContext> readModelContextFactory)
        {
            this.app = new Application(commandBus, workerRoleStatusUrl, readModelContextFactory, 10);
            this.buggyApp = new Application(new BuggyCommandBus(), workerRoleStatusUrl, readModelContextFactory, 1);
        }

        public void AddItem(string name)
        {
            var command = new AddItem(SequentialGuid.GenerateNewGuid());
            this.app.SendCommand(command);
        }

        public void AddItemBuggyProcess(string name)
        {
            var command = new AddItem(SequentialGuid.GenerateNewGuid());
            this.buggyApp.SendCommand(command);
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

    public class AddItem : Command
    {
        public AddItem(Guid id)
            : base(id)
        { }
    }
}
