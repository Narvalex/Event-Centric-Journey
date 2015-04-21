
namespace Journey.EventSourcing.ReadModeling
{
    public interface IReadModelRebuilder<T> where T : ReadModelDbContext
    {
        void Rebuild();
    }
}
