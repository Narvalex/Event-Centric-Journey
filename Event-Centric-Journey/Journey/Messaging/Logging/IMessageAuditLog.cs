namespace Journey.Messaging.Logging
{
    public interface IMessageAuditLog
    {
        void Log(IEvent @event);

        void Log(ICommand command);

        bool IsDuplicateMessage(IEvent @event);

        bool IsDuplicateMessage(ICommand command);
    }
}
