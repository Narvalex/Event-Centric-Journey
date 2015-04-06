using Journey.EventSourcing;
using Journey.EventSourcing.Handling;
using SimpleInventario.Commands;
using SimpleInventario.Events;
using System;
using System.Collections.Generic;

namespace SimpleInventario
{
    public class Inventario : EventSourced, IMementoOriginator,
        IHandlerOf<DefinirNuevoTipoDeArticulo>
    {
        public Inventario(Guid id)
            : base(id)
        {
            base.RehydratesFrom<NuevoTipoDeArticuloDefinido>(this.OnNuevoTipoDeArticuloDefinido);
        }

        public Inventario(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            base.LoadFrom(history);
        }

        public Inventario(Guid id, IMemento memento, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            var state = memento as Memento;
            base.Version = state.Version;
            // make a copy of the state values to avoid concurrency problems with reusing references.
            //this.remainingSeats.AddRange(state.RemainingSeats);
            //this.pendingReservations.AddRange(state.PendingReservations);
            this.LoadFrom(history);
        }

        public void Handle(DefinirNuevoTipoDeArticulo c)
        {
            base.Update(new NuevoTipoDeArticuloDefinido
                {
                    IdArticulo = c.IdArticulo,
                    Nombre = c.Nombre
                });
        }

        private void OnNuevoTipoDeArticuloDefinido(NuevoTipoDeArticuloDefinido e)
        { }

        public IMemento SaveToMemento()
        {
            return new Memento
            {
                Version = this.Version
            };
        }

        internal class Memento : IMemento
        {
            public int Version { get; internal set; }
        }
    }
}
