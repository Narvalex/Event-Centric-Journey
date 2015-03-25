using System;

namespace Infrastructure.CQRS.Messaging
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(Message message)
        {
            this.Message = message;            
        }

        public Message Message { get; private set; }
    }
}
