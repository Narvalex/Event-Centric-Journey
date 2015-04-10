using Journey.Client;
using Journey.Messaging;
using System.Collections.Generic;

namespace Journey.Tests.Testing.Fakes
{
    public class FakeApp : IClientApplication
    {
        public FakeApp()
        {
            this.Commands = new List<ICommand>();
        }

        public void Send(ICommand command)
        {
            this.Commands.Add(command);
        }

        public List<ICommand> Commands { get; set; }
    }
}
