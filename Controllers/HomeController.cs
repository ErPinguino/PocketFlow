using Microsoft.AspNetCore.Mvc;
using PocketFlow.Models;
using System.Diagnostics;

namespace PocketFlow.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account");
            }

            var onboardingClaim = User.FindFirst("onboarding_completed")?.Value;
            if (onboardingClaim == "true")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return RedirectToAction("Index", "Onboarding");
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
