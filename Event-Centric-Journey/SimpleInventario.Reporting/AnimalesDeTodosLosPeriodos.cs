using Journey.EventSourcing;
using Journey.Utils;
using SimpleInventario.Events;
using SimpleInventario.Reporting.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleInventario.Reporting
{
    /// <summary>
    /// No se deberia crear uno que diga reporting. Quizas esto se utilice como logica para otros procesos y 
    /// no quede simplemente como un modulo netamente de reportes.
    /// </summary>
    /// <remarks>
    /// Is an <see cref="IMementoOriginator"/>
    /// </remarks>
    public class AnimalesDeTodosLosPeriodos : ComplexEventSourced, IMementoOriginator,
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
            var state = memento as AnimalesDeTodosLosPeriodosMemento;
            base.Version = state.Version;
            base.lastProcessedEvents.AddRange(state.LastProcessedEvents);
            base.earlyReceivedEvents.AddRange(state.EarlyReceivedEvents);
            // make a copy of the state values to avoid concurrency problems with reusing references.
            // uses an extension method
            this.animalesPorPeriodo.AddRange(state.AnimalesPorPeriodo);
            base.LoadFrom(history);
        }

        public IMemento SaveToMemento()
        {
            return new AnimalesDeTodosLosPeriodosMemento(base.Version, base.lastProcessedEvents.ToArray(), base.earlyReceivedEvents.ToArray(), this.animalesPorPeriodo.ToArray());
        }

        public class AnimalesDeTodosLosPeriodosMemento : ComplexMemento
        {
            public AnimalesDeTodosLosPeriodosMemento(int version, KeyValuePair<string, int>[] lastProcessedEvents, IVersionedEvent[] earlyReceivedEvents,
                KeyValuePair<int, int>[] animalesPorPeriodo)
                : base(version, lastProcessedEvents, earlyReceivedEvents)
            {
                this.AnimalesPorPeriodo = animalesPorPeriodo;
            }

            public KeyValuePair<int, int>[] AnimalesPorPeriodo { get; private set; }
        }

        public void Process(SeAgregaronAnimalesAlInventario e)
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
