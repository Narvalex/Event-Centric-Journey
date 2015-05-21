using Journey.Messaging;
using System;

namespace Journey.Tests.Integration.EventSourcing.Helpers
{
    public class FakeCommand : ExternalCommand
    {
        public FakeCommand(Guid id)
            : base(id)
        { }
    }
}
