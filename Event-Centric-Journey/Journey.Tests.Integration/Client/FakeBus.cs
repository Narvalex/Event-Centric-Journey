using Journey.Messaging;
using System;
using System.Collections.Generic;

namespace Journey.Tests.Integration.Client
{
    public class FakeBus : ICommandBus, IEventBus
    {
        public void Publish(Envelope<IEvent> @event)
        {
            throw new NotImplementedException();
        }

        public void Publish(IEnumerable<Envelope<IEvent>> events)
        {
            throw new NotImplementedException();
        }

        public void Publish(IEnumerable<Envelope<IEvent>> events, System.Data.Entity.DbContext context)
        {
            throw new NotImplementedException();
        }

        public void Send(Envelope<ICommand> command)
        {
            throw new NotImplementedException();
        }

        public void Send(IEnumerable<Envelope<ICommand>> commands)
        {
            throw new NotImplementedException();
        }

        public void Send(IEnumerable<Envelope<ICommand>> commands, System.Data.Entity.DbContext context)
        {
            throw new NotImplementedException();
        }
    }
}
