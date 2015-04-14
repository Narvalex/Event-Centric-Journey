using Journey.EventSourcing;
using Journey.Utils;
using SimpleInventario.Events;
using SimpleInventario.Reporting.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleInventario.Reporting
{
    public class AnimalesDeTodosLosPeriodos : EventSourced, IMementoOriginator,
        ISubscribedTo<SeAgregaronAnimalesAlInventario>,
        IRehydratesFrom<SeActualizoResumenDeAnimalesPorPeriodo>
    {
        private readonly IDictionary<int, int> animalesPorPeriodo = new Dictionary<int, int>();

        public AnimalesDeTodosLosPeriodos(Guid id)
            : base(id)
        { }

        public AnimalesDeTodosLosPeriodos(Guid id, IEnumerable<IVersionedEvent> history)
            : base(id)
        {
            base.LoadFrom(history);
        }

        public AnimalesDeTodosLosPeriodos(Guid id, IMemento memento, IEnumerable<IVersionedEvent> history)
            : base(id)
        {
            var state = memento as Memento;
            base.Version = state.Version;
            // make a copy of the state values to avoid concurrency problems with reusing references.
            // uses an extension method
            this.animalesPorPeriodo.AddRange(state.AnimalesPorPeriodo);
            base.LoadFrom(history);
        }

        public IMemento SaveToMemento()
        {
            return new Memento
            {
                Version = this.Version,
                AnimalesPorPeriodo = this.animalesPorPeriodo.ToArray()
            };
        }

        internal class Memento : IMemento
        {
            public int Version { get; internal set; }
            internal KeyValuePair<int, int>[] AnimalesPorPeriodo { get; set; }
        }

        public void Consume(SeAgregaronAnimalesAlInventario e)
        {
            this.ComprobarPeriodo(e.Periodo);

            this.animalesPorPeriodo[e.Periodo] += e.Cantidad;

            base.Update(new SeActualizoResumenDeAnimalesPorPeriodo
                {
                    Periodo = e.Periodo,
                    CantidadDeAnimales = this.animalesPorPeriodo[e.Periodo]
                });
        }

        public void Rehydrate(SeActualizoResumenDeAnimalesPorPeriodo e)
        {
            this.ComprobarPeriodo(e.Periodo);

            this.animalesPorPeriodo[e.Periodo] = e.CantidadDeAnimales;
        }

        private void ComprobarPeriodo(int periodo)
        {
            // Habilitamos implicitamente un periodo. Implicito por que no lo hacemos
            // a travez de un nuevo evento.
            if (!this.animalesPorPeriodo.ContainsKey(periodo))
                this.animalesPorPeriodo.Add(new KeyValuePair<int, int>(periodo, 0));
        }
    }
}
