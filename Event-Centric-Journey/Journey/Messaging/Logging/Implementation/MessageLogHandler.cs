
namespace Journey.Messaging.Logging
{
    /// <summary>
    /// The SQL version of the event log runs directly in-proc
    /// and is implemented as an event and command handler instead of a 
    /// raw message listener.
    /// Is currently deprecated since the message log is now a core component of the Event Store
    /// </summary>
    //public class MessageLogHandler : IEventHandler<IEvent>, ICommandHandler<ICommand>
    //{
    //    private IMessageAuditLog logger;

    //    public MessageLogHandler(IMessageAuditLog logger)
    //    {
    //        this.logger = logger;
    //    }

    //    public void Handle(IEvent @event)
    //    {
    //        this.logger.Log(@event);
    //    }

    //    public void Handle(ICommand command)
    //    {
    //        this.logger.Log(command);
    //    }
    //}
}
