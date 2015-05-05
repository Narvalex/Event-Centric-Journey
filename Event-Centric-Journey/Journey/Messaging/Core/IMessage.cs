using System;

namespace Journey.Messaging
{
    public interface IMessage
    {
        DateTime CreationDate { get; set; }
    }
}
