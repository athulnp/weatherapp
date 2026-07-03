using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WeatherApp.IService;
using WeatherApp.Models;
using WeatherApp.Service;

namespace WeatherApp.Controllers
{
    public class CitiesController : Controller
    {
        private readonly ILogger<CitiesController> _logger;
        private readonly IWeatherService _weatherService;
        private readonly CitySearchService _citySearchService;
        private readonly CountryCitiesService _countryCitiesService;

        public CitiesController(ILogger<CitiesController> logger, IWeatherService weatherService, CitySearchService citySearchService, CountryCitiesService countryCitiesService)
        {
            _logger = logger;
            _weatherService = weatherService;
            _citySearchService = citySearchService;
            _countryCitiesService = countryCitiesService;
        }

        [HttpGet]
        [Route("weather/{cityName}")]
        [Route("{culture:regex(^(hi|ta|ml)$)}/weather/{cityName}")]
        public async Task<IActionResult> Index(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Normalize city name (e.g., convert to title case)
                var normalizedCityName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cityName.ToLower().Replace("-", " "));

                // Stable, lowercase-hyphenated slug for canonical URLs so that
                // /weather/Mumbai and /weather/mumbai resolve to one canonical.
                var citySlug = normalizedCityName.ToLower().Replace(" ", "-");

                // Record the search
                _citySearchService.RecordSearch(normalizedCityName);
                
                // Get weather data for the city
                var weatherData = await _weatherService.GetWeatherAsync(normalizedCityName);
                
                if (weatherData == null)
                {
                    _logger.LogWarning($"Weather data not found for city: {normalizedCityName}");
                    return RedirectToAction("Index", "Home");
                }

                // Get popular cities for the country
                var popularCities = new List<CitySearchResult>();
                
                if (!string.IsNullOrEmpty(weatherData.CountryCode))
                {
                    // Try to fetch cities from REST Countries API
                    var geoCities = await _countryCitiesService.GetPopularCitiesByCountry(weatherData.CountryCode);
                    if (geoCities.Any())
                    {
                        popularCities = geoCities.Take(6).Select(c => new CitySearchResult
                        {
                            CityName = c.Name,
                            SearchCount = 0
                        }).ToList();
                    }
                    else
                    {
                        // Fallback to predefined cities
                        var fallbackCities = _countryCitiesService.GetFallbackCities(weatherData.CountryCode);
                        popularCities = fallbackCities.Take(6).Select(c => new CitySearchResult
                        {
                            CityName = c.Name,
                            SearchCount = 0
                        }).ToList();
                    }
                }
                
                // If still no cities, use global popular cities
                if (!popularCities.Any())
                {
                    popularCities = _citySearchService.GetPopularCities(6);
                }

                // Create SEO model with meta tags and structured data
                var viewModel = new CityWeatherViewModel
                {
                    WeatherData = weatherData,
                    CityName = normalizedCityName,
                    PageTitle = $"Weather in {normalizedCityName} - Current Conditions & Forecast | Kairos Weather",
                    MetaDescription = $"Get current weather conditions and forecast for {normalizedCityName}. Check temperature, humidity, wind speed, and more with Kairos Weather's accurate real-time updates.",
                    MetaKeywords = $"{normalizedCityName} weather, weather in {normalizedCityName}, {normalizedCityName} forecast, current weather {normalizedCityName}, {normalizedCityName} temperature",
                    CanonicalUrl = $"https://kairosweather.info/weather/{citySlug}",
                    PopularCities = popularCities
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading weather data for city: {cityName}");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [Route("weather/{cityName}/forecast")]
        [Route("{culture:regex(^(hi|ta|ml)$)}/weather/{cityName}/forecast")]
        public async Task<IActionResult> Forecast(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var normalizedCityName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cityName.ToLower().Replace("-", " "));
                var citySlug = normalizedCityName.ToLower().Replace(" ", "-");
                var weatherData = await _weatherService.GetWeatherAsync(normalizedCityName);

                if (weatherData == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var forecast = await _weatherService.GetWeatherForecast(weatherData.Latitude, weatherData.Longitude);

                var viewModel = new CityWeatherViewModel
                {
                    WeatherData = weatherData,
                    ForecastData = forecast,
                    CityName = normalizedCityName,
                    PageTitle = $"5-Day Weather Forecast for {normalizedCityName} | Kairos Weather",
                    MetaDescription = $"View the 5-day weather forecast for {normalizedCityName}. Plan your week with accurate temperature, precipitation, and weather predictions.",
                    MetaKeywords = $"{normalizedCityName} forecast, 5-day weather {normalizedCityName}, weather prediction {normalizedCityName}, weekly forecast {normalizedCityName}",
                    CanonicalUrl = $"https://kairosweather.info/weather/{citySlug}/forecast"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading forecast for city: {cityName}");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
