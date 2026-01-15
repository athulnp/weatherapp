using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WeatherApp.IService;
using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWeatherService _weatherService;

        public HomeController(ILogger<HomeController> logger, IWeatherService weatherService)
        {
            _logger = logger;
            _weatherService = weatherService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new Weather();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(Weather weather)
        {
            var weatherData = await _weatherService.GetWeatherAsync(weather.CityName);
            return View(weatherData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About() => View();
        public IActionResult PrivacyPolicy() => View();
        public IActionResult Terms() => View();
        public IActionResult Contact() => View();


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
