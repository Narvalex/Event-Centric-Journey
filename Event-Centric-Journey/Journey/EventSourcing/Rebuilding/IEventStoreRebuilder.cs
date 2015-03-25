
namespace Infrastructure.CQRS.EventSourcing.Rebuilding
{
    public interface IEventStoreRebuilder
    {
        void Rebuild(EventStoreDbContext eventStoreDbContext);
    }
}
