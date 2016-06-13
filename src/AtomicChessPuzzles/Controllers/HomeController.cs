using Microsoft.AspNet.Mvc;

namespace AtomicChessPuzzles
{
    public class HomeController : Controller
    {
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
