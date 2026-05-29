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

        [HttpGet]
        public async Task<IActionResult> GetWeatherByCoordinates(double lat, double lon)
        {
            try
            {
                var coordinates = $"{lat},{lon}";
                var weatherData = await _weatherService.GetWeatherAsync(coordinates);
                return Json(new { success = true, data = weatherData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather by coordinates");
                return Json(new { success = false, error = "Failed to get weather data" });
            }
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
