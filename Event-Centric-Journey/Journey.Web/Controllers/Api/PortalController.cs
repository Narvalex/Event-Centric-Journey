using Journey.Worker;
using System.Web.Http;

namespace Journey.Web.Controllers.Api
{
    public class PortalController : ApiController
    {
        private readonly IWorkerRolePortal portal;

        public PortalController()
        {
            this.portal = WorkerRoleWebPortal.Instance;
        }

        [HttpGet]
        [Route("api/portal/status")]
        public IHttpActionResult Status()
        {
            return this.Ok(this.portal.IsWorking);
        }

        [HttpGet]
        [Route("api/portal/start")]
        public IHttpActionResult Start()
        {
            this.portal.StartWorking();
            return this.Ok(this.portal.IsWorking);
        }

        [HttpGet]
        [Route("api/portal/stop")]
        public IHttpActionResult Stop()
        {
            this.portal.StopWorking();
            return this.Ok(this.portal.IsWorking);
        }
    }
}
