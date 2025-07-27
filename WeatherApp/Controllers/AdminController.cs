using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using WeatherApp.IService;
using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IWeatherService _weatherService;

        public AdminController(ILogger<AdminController> logger, IWeatherService weatherService)
        {
            _logger = logger;
            _weatherService = weatherService;
        }


        public IActionResult Login(string returnUrl = "/")
        {
           return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, "Auth0");
        }


        public IActionResult Logout()
        {
            return SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme,
                "Auth0");
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            var user = new User
            {
                Name = User.Identity.Name,
                Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            };
            return View(user);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
