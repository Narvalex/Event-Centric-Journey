
namespace Journey.EventSourcing.Rebuilding
{
    public interface IEventStoreRebuilder
    {
        void Rebuild(EventStoreDbContext eventStoreDbContext);
    }
}
