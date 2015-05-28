
namespace Journey.EventSourcing
{
    /// <summary>
    /// Marker interface that defines a projectable event. This helps improve performance on
    /// projection rebuilding.
    /// </summary>
    public interface IProjectableEvent
    { }
}
