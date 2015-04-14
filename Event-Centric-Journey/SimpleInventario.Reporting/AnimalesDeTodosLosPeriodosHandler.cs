using Journey.EventSourcing;
using Journey.Messaging.Processing;
using SimpleInventario.Events;

namespace SimpleInventario.Reporting
{
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
            var aggregate = this.store.Find(e.IdEmpresa);
            if (aggregate == null)
                aggregate = new AnimalesDeTodosLosPeriodos(e.IdEmpresa);

            aggregate.Consume(e);
            this.store.Save(aggregate, e.CorrelationId);
        }
    }
}
