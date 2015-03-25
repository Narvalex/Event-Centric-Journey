using Journey.Messaging;
using System.Collections.Generic;

namespace Journey.EventSourcing
{
    /// <summary>
    /// Interface implemented by Sagas (also known as Process Managers in the CQRS community) that 
    /// publish commands to the bus.
    /// </summary>
    /// <remarks>
    /// <para>See <see cref="http://go.microsoft.com/fwlink/p/?LinkID=258564">Reference 6</see> for a description of what is a Process Manager.</para>
    /// <para>There are a few things that we learnt along the way regarding Process Managers, which we might do differently with the new insights that we
    /// now have. See <see cref="http://go.microsoft.com/fwlink/p/?LinkID=258558"> Journey lessons learnt</see> for more information.</para>
    /// </remarks>
    public interface ISaga : IEventSourced
    {
        /// <summary>
        /// Gets a collection of commands that need to be sent when the state of the Saga is persisted.
        /// </summary>
        IEnumerable<ICommand> Commands { get; }
    }
}
