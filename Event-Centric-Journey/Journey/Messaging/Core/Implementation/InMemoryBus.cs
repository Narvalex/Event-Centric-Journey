using System.Collections.Generic;

namespace Journey.Messaging
{
    public class InMemoryBus : IInMemoryBus
    {
        public void Send(IEnumerable<ICommand> commands)
        {
            throw new System.NotImplementedException();
        }

        public void Publish(IEnumerable<IEvent> events)
        {
            throw new System.NotImplementedException();
        }
    }
}
