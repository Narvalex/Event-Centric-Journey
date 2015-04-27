
namespace Journey.EventSourcing.ReadModeling
{
    public interface IReadModelRebuilderEngine<T> where T : ReadModelDbContext
    {
        void Rebuild();
    }
}
