using ARBISTO_POS.Attributes;
using ARBISTO_POS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ARBISTO_POS.Controllers
{
    [Permission("Access Dashboard")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
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
