using System.Web.Mvc;

namespace Journey.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return this.Redirect("/Portal");
        }
    }
}