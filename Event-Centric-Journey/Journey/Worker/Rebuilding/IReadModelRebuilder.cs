using Journey.EventSourcing.ReadModeling;
namespace Journey.Worker.Rebuilding
{
    public interface IReadModelRebuilder<T> where T : ReadModelDbContext
    {
        void Rebuild();

        IReadModelRebuilderPerfCounter PerformanceCounter { get; }
    }
}
