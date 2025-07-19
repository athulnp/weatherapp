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
            var weatherData = new Weather();
            try
            {
                weatherData = await _weatherService.GetWeatherData(weather.CityName);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to fetch weather data: {Message}", ex.Message);
            }

            return View(weatherData);
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
