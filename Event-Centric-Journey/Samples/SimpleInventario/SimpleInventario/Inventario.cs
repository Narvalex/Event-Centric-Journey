using Journey.EventSourcing;
using Journey.EventSourcing.Handling;
using SimpleInventario.Commands;
using SimpleInventario.Events;
using System;
using System.Collections.Generic;

namespace SimpleInventario
{
    public class Inventario : EventSourced, IMementoOriginator,
        IHandlerOf<AgregarAnimales>,
        IRehydratesFrom<SeAgregaronAnimalesAlInventario>
    {
        public Inventario(Guid id)
            : base(id)
        { }

        public Inventario(Guid id, IEnumerable<IVersionedEvent> history)
            : base(id)
        {
            base.LoadFrom(history);
        }

        public Inventario(Guid id, IMemento memento, IEnumerable<IVersionedEvent> history)
            : base(id)
        {
            var state = memento as Memento;
            base.Version = state.Version;
            // make a copy of the state values to avoid concurrency problems with reusing references.
            //this.remainingSeats.AddRange(state.RemainingSeats);
            //this.pendingReservations.AddRange(state.PendingReservations);
            base.LoadFrom(history);
        }

        public void Handle(AgregarAnimales c)
        {
            base.Update(new SeAgregaronAnimalesAlInventario
                {
                    IdEmpresa = c.IdEmpresa,
                    Animal = c.Animal,
                    Sucursal = c.Sucursal,
                    Cantidad = c.Cantidad,
                    Periodo = c.Periodo
                });
        }

        public void Rehydrate(SeAgregaronAnimalesAlInventario e)
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
