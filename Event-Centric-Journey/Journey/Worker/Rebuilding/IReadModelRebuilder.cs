using Journey.EventSourcing.ReadModeling;
using Journey.EventSourcing.RebuildPerfCounting;
namespace Journey.Worker.Rebuilding
{
    public interface IReadModelRebuilder<T> where T : ReadModelDbContext
    {
        void Rebuild();

        IRebuilderPerfCounter PerformanceCounter { get; }
    }
}
