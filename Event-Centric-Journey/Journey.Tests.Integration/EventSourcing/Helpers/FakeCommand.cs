using Journey.Messaging;
using System;

namespace Journey.Tests.Integration.EventSourcing.Helpers
{
    public class FakeCommand : Command
    {
        public FakeCommand(Guid id)
            : base(id)
        { }
    }
}
