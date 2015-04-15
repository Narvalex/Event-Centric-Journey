
using SimpleInventario.Querying.DTOs;
using System.Collections.Generic;
namespace SimpleInventario.Querying
{
    public interface ISimpleInventarioDao
    {
        IList<CantidadDeAnimalesDeUnPeriodo> GetResumenDeCantidadDeAnimalesPorPeriodo();
    }
}
