using SimpleInventario.Querying;
using System.Web.Http;

namespace SimpleInventario.Web.Querying
{
    public class AdminQueryController : ApiController
    {
        private readonly ISimpleInventarioDao dao;

        public AdminQueryController(ISimpleInventarioDao dao)
        {
            this.dao = dao;
        }

        [HttpGet]
        [Route("api/adminQuery/getResumenDeCantidadDeAnimalesPorPeriodo")]
        public IHttpActionResult GetResumenDeCantidadDeAnimalesPorPeriodo()
        {
            var resumen = this.dao.GetResumenDeCantidadDeAnimalesPorPeriodo();
            return this.Ok(resumen);
        }
    }
}
