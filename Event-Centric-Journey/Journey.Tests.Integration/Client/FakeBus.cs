using Journey.Messaging;
using System;
using System.Collections.Generic;

namespace Journey.Tests.Integration.Client
{
    public class FakeBus : ICommandBus, IEventBus
    {
        public void Publish(Envelope<IEvent> @event)
        {
            
        }

        public void Publish(IEnumerable<Envelope<IEvent>> events)
        {
            
        }

        public void Publish(IEnumerable<Envelope<IEvent>> events, System.Data.Entity.DbContext context)
        {
            
        }

        public void Send(Envelope<ICommand> command)
        {
            
        }

        public void Send(IEnumerable<Envelope<ICommand>> commands)
        {
            
        }

        public void Send(IEnumerable<Envelope<ICommand>> commands, System.Data.Entity.DbContext context)
        {
            
        }
    }
}
