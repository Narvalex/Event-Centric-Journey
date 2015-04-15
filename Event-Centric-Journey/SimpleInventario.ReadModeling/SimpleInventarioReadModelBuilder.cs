using Journey.EventSourcing.ReadModeling;
using Journey.Messaging.Processing;
using SimpleInventario.ReadModel;
using SimpleInventario.ReadModel.Entities;
using SimpleInventario.Reporting.Events;

namespace SimpleInventario.ReadModeling
{
    public class SimpleInventarioReadModelBuilder :
        IEventHandler<SeActualizoResumenDeAnimalesPorPeriodo>
    {
        private readonly IReadModelGenerator<SimpleInventarioDbContext> generator;

        public SimpleInventarioReadModelBuilder(IReadModelGenerator<SimpleInventarioDbContext> generator)
        {
            this.generator = generator;
        }

        public void Handle(SeActualizoResumenDeAnimalesPorPeriodo e)
        {
            this.generator.Project(e,
            (context) =>
            {
                context.AddToUnitOfWork<CantidadDeAnimalesDeUnPeriodo>(
                    new CantidadDeAnimalesDeUnPeriodo
                    {
                        Periodo = e.Periodo.ToString(),
                        Cantidad = e.CantidadDeAnimales
                    });
            });
        }
    }
}
