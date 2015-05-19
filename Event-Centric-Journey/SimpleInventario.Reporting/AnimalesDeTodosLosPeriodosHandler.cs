using Journey.EventSourcing;
using Journey.Messaging.Processing;
using SimpleInventario.Events;

namespace SimpleInventario.Reporting
{
    /// <summary>
    /// No se deberia crear uno que diga reporting. Quizas esto se utilice como logica para otros procesos y 
    /// no quede simplemente como un modulo netamente de reportes.
    /// </summary>
    public class AnimalesDeTodosLosPeriodosHandler
        : IEventHandler<SeAgregaronAnimalesAlInventario>
    {
        private readonly IEventStore<AnimalesDeTodosLosPeriodos> store;

        public AnimalesDeTodosLosPeriodosHandler(IEventStore<AnimalesDeTodosLosPeriodos> store)
        {
            this.store = store;
        }

        public void Handle(SeAgregaronAnimalesAlInventario e)
        {
            lock (this)
            {
                //Thread.Sleep(TimeSpan.FromDays(2));

                var aggregate = this.store.Find(e.IdEmpresa);
                if (aggregate == null)
                    aggregate = new AnimalesDeTodosLosPeriodos(e.IdEmpresa);

                if (aggregate.TryProcessWithGuaranteedIdempotency(e))
                    this.store.Save(aggregate, e);
            }
        }
    }
}
