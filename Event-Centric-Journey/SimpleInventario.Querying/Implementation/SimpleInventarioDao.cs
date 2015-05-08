using SimpleInventario.Querying.DTOs;
using SimpleInventario.ReadModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleInventario.Querying
{
    public class SimpleInventarioDao : ISimpleInventarioDao
    {
        private readonly Func<SimpleInventarioDbContext> contextFactory;

        public SimpleInventarioDao(Func<SimpleInventarioDbContext> contextFactory)
        {
            // performance optimizer.
            this.contextFactory = () =>
            {
                var context = contextFactory.Invoke();
                context.Configuration.AutoDetectChangesEnabled = false;
                return context;
            };
        }

        public IList<CantidadDeAnimalesDeUnPeriodo> GetResumenDeCantidadDeAnimalesPorPeriodo()
        {
            using (var context = this.contextFactory.Invoke())
            {
                return context.ResumenDeAnimalesDeTodosLosPeriodos
                    .Select(x => new CantidadDeAnimalesDeUnPeriodo
                    {
                        Periodo = x.Periodo,
                        CantidadDeAnimales = x.Cantidad
                    })
                    .ToList();
            }
        }
    }
}
