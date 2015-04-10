using SimpleInventario.Application;
using SimpleInventario.Application.DTOs;
using System.Web.Http;

namespace SimpleInventario.Web.Comanding
{
    public class AdminController : ApiController
    {
        private readonly IInventarioApp app;

        public AdminController(IInventarioApp app)
        {
            this.app = app;
        }

        [HttpPost]
        [Route("api/admin/agregarAnimales")]
        public IHttpActionResult AgregarAnimales([FromBody]AgregarAnimalesDto dto)
        {
            this.app.AgregarAnimales(dto);
            return this.Ok();
        }
    }
}
