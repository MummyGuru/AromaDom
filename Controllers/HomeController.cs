using System.Web.Mvc;

namespace AromaDom.Controllers
{
    /// Контроллер главной страницы. Перенаправляет на список материалов.
    public class HomeController : Controller
    {
        // GET: /
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Materials");
        }
    }
}
