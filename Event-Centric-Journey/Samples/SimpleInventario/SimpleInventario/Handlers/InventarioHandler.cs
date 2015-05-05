using Journey.EventSourcing;
using Journey.Messaging.Processing;
using SimpleInventario.Commands;

namespace SimpleInventario.Handlers
{
    public class InventarioHandler :
        ICommandHandler<AgregarAnimales>
    {
        private readonly IEventStore<Inventario> store;

        public InventarioHandler(IEventStore<Inventario> store)
        {
            this.store = store;
        }

        public void Handle(AgregarAnimales command)
        {
            var actor = this.store.Find(command.IdEmpresa);
            if (actor == null)
                actor = new Inventario(command.IdEmpresa);

            actor.Handle(command);
            this.store.Save(actor, command.Id, command.CreationDate);
        }
    }
}
