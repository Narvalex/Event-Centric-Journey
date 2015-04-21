using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using SimpleInventario.ReadModel;
using SimpleInventario.ReadModel.Entities;
using SimpleInventario.Reporting.Events;
using System;
using System.Linq;

namespace SimpleInventario.ReadModeling
{
    public class SimpleInventarioReadModelGenerator :
        IEventHandler<SeActualizoResumenDeAnimalesPorPeriodo>
    {
        private readonly IReadModelGeneratorEngine<SimpleInventarioDbContext> generator;

        public SimpleInventarioReadModelGenerator(IReadModelGeneratorEngine<SimpleInventarioDbContext> generator)
        {
            this.generator = generator;
        }

        public void Handle(SeActualizoResumenDeAnimalesPorPeriodo e)
        {
            Action<SimpleInventarioDbContext, CantidadDeAnimalesDeUnPeriodo> 
                actualizarResumenDeAnimalesPorPeriodo =
                    (context, registroExistente) =>
                    {
                        if (registroExistente == null)
                            // es un registro nuevo
                            context.AddToUnitOfWork<CantidadDeAnimalesDeUnPeriodo>(
                                new CantidadDeAnimalesDeUnPeriodo
                                {
                                    Periodo = e.Periodo.ToString(),
                                    Cantidad = e.CantidadDeAnimales
                                });
                        else
                        {
                            // El periodo ya esta registrado, actualizamos las cantidades
                            registroExistente.Cantidad = e.CantidadDeAnimales;
                            context.AddToUnitOfWork<CantidadDeAnimalesDeUnPeriodo>(registroExistente);
                        };
                    };

            this.generator.Project(e,
            (context) =>
            {
                // verificamos si existe ya un registro para el periodo
                var registroExistente =
                    context
                    .ResumenDeAnimalesDeTodosLosPeriodos
                    .Where(x => x.Periodo == e.Periodo.ToString())
                    .FirstOrDefault();

                actualizarResumenDeAnimalesPorPeriodo(context, registroExistente);
            },
            (context) =>
            {
                // verificamos si existe ya un registro para el periodo
                var registroExistente =
                    context
                    .ResumenDeAnimalesDeTodosLosPeriodos
                    .Local
                    .Where(x => x.Periodo == e.Periodo.ToString())
                    .FirstOrDefault();

                actualizarResumenDeAnimalesPorPeriodo(context, registroExistente);
            });
        }

    }
}
