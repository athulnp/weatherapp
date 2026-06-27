using Microsoft.AspNetCore.Mvc;

namespace WeatherApp.Controllers
{
    public class StaticController : Controller
    {
        public IActionResult About() => View();
        public IActionResult PrivacyPolicy() => View();
        public IActionResult Privacy() => RedirectToActionPermanent(nameof(PrivacyPolicy));
        public IActionResult Terms() => View();
        public IActionResult Contact() => View();
        public IActionResult AdDisclosure() => View();
        public IActionResult FAQ() => View();
    }
}
