using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SIMS_APDP.Models;

namespace SIMS_APDP.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Test()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
