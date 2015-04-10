using System.Web.Mvc;

namespace SimpleInventario.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}