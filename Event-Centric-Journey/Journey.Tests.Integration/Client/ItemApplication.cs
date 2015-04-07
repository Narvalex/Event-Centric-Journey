using Journey.Client;
using Journey.Messaging;
using System;

namespace Journey.Tests.Integration.Client
{
    public class ItemApplication
    {
        private readonly IApplication app;

        public ItemApplication(IApplication app)
        {
            this.app = app;
        }

        public void AddItem(string name)
        {
            var command = new AddItem(Guid.Empty);
            this.app.SendCommand(command);
        }
    }

    public class AddItem : Command
    {
        public AddItem(Guid id)
            : base(id)
        { }
    }
}
