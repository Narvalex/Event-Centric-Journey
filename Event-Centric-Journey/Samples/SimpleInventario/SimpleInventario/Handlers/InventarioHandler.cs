using Journey.EventSourcing;
using Journey.Messaging.Processing;
using SimpleInventario.Commands;

namespace SimpleInventario.Handlers
{
    public class InventarioHandler : 
        ICommandHandler<DefinirNuevoTipoDeArticulo>
    {
        private readonly IEventStore<Inventario> store;

        public InventarioHandler(IEventStore<Inventario> store)
        {
            this.store = store;
        }

        public void Handle(DefinirNuevoTipoDeArticulo command)
        {
            var aggregate = new Inventario(command.IdArticulo);
            aggregate.Handle(command);
            this.store.Save(aggregate, command.Id);
        }
    }
}
