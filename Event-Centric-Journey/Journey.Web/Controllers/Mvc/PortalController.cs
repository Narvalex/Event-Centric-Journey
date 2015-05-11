using Journey.Worker;
using Journey.Worker.Portal;
using System.Web.Mvc;

namespace Journey.Web.Controllers
{
    public class PortalController : Controller
    {
        // GET: Portal
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WorkerRoleStatus(string requester)
        {
            WorkerRoleWebPortal.Instance.WorkerRole.Tracer.TraceAsync("========== INCOMING STATUS CHECK BY: " + requester + " ==========");

            return Content("Online");
        }
    }
}