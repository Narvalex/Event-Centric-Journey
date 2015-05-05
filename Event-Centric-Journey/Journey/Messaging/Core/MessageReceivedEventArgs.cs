using System;

namespace Journey.Messaging
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(MessageForDelivery message)
        {
            this.Message = message;            
        }

        public MessageForDelivery Message { get; private set; }
    }
}
