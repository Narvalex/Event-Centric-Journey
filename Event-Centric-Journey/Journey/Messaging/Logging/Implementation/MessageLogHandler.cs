using Infrastructure.CQRS.Messaging.Processing;

namespace Infrastructure.CQRS.Messaging.Logging
{
    /// <summary>
    /// The SQL version of the event log runs directly in-proc
    /// and is implemented as an event and command handler instead of a 
    /// raw message listener.
    /// </summary>
    public class MessageLogHandler : IEventHandler<IEvent>, ICommandHandler<ICommand>
    {
        private MessageLog log;

        public MessageLogHandler(MessageLog log)
        {
            this.log = log;
        }

        public void Handle(IEvent @event)
        {
            this.log.Save(@event);
        }

        public void Handle(ICommand command)
        {
            this.log.Save(command);
        }
    }
}
