using Journey.EventSourcing.ReadModeling;
using Journey.Messaging;
using System;
using System.Collections.Generic;

namespace Journey.Tests.Integration.Client
{
    public class FakeBus : ICommandBus, IEventBus
    {
        private Func<ReadModelDbContext> contextFactory;

        public FakeBus(Func<ReadModelDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

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
            using (var context = this.contextFactory.Invoke())
            {
                context
                    .ReadModeling
                    .Add(new ProjectedEvent
                    {
                        AggregateId = Guid.Empty, 
                        AggregateType = "Items",
                        Version = 1,
                        CorrelationId = command.Body.Id
                    });

                context.SaveChanges();
            }
        }

        public void Send(IEnumerable<Envelope<ICommand>> commands)
        {
            
        }

        public void Send(IEnumerable<Envelope<ICommand>> commands, System.Data.Entity.DbContext context)
        {
            
        }
    }
}
