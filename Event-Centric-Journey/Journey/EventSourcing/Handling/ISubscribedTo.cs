﻿using Journey.Messaging;

namespace Journey.EventSourcing
{
    public interface ISubscribedTo { }

    public interface ISubscribedTo<T> : ISubscribedTo
        where T : IEvent
    {
        void Consume(T e);
    }
}
