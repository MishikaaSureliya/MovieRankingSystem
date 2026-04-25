using Microsoft.AspNetCore.Mvc;

namespace MovieRankingSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View(); // 🔥 REQUIRED
        }

        public IActionResult Register()
        {
            return View();
        }
        public IActionResult Search()
        {
            return View();
        }
        public IActionResult Compare()
        {
            return View();
        }
    }
}
