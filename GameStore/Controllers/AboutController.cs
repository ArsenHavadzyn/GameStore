using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Guarantee()
        {
            return View();
        }

        public IActionResult Delivery()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }
    }
}
