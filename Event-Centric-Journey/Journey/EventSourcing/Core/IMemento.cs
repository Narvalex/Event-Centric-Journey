namespace Infrastructure.CQRS.EventSourcing
{
    /// <summary>
    /// An opaque object that contains the state of another object (a snapshot) and can be used to restore its state.
    /// </summary>
    public interface IMemento
    {
        /// <summary>
        /// The version of the <see cref="IEventSourced"/> instance.
        /// </summary>
        int Version { get; }
    }
}
