﻿
namespace Journey.EventSourcing
{
    /// <summary>
    /// Un evento versionado. Este tipo de evento es extendido cuando 
    /// por un implementador que se utiliza para persitir eventos versionados 
    /// en el Depósito de Eventos (Event Store).
    /// </summary>
    public abstract class ExternalVersionedEvent : VersionedEvent, IVersionedEvent
    {
        public ExternalVersionedEvent()
        {
            this.IsExternal = false;
        }

        public bool IsExternal { get; private set; }
    }
}
