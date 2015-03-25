using System;

namespace Journey.Messaging
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
