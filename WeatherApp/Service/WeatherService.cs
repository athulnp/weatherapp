using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NuGet.Packaging.Licenses;
using System.Text.RegularExpressions;
using WeatherApp.Helper;
using WeatherApp.IService;
using WeatherApp.Models;

namespace WeatherApp.Service
{
    public class WeatherService : IWeatherService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        private readonly ILogger<WeatherService> _logger;
        public WeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ICacheService cacheService, ILogger<WeatherService> logger)
        {
            _httpClientFactory = httpClientFactory ;
            _configuration = configuration;
            _cacheService = cacheService;
            _logger = logger;
        }

        // Implement methods from IWeatherService here
        // For example, you might have methods to fetch weather data from an API
        public async Task<Weather> GetWeatherData(string location)
        {
            var geoLocationList =  new List<GeoLocation>();
            // Simulate fetching weather data
            //http://api.openweathermap.org/geo/1.0/direct?q=London&limit=5&appid=8c3a7a1fedef1ca1d1261de353d2698f
            // In a real application, you would make an HTTP request to a weather API
            if (string.IsNullOrEmpty(location))
            {
                throw new ArgumentException("Location cannot be null or empty", nameof(location));
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.openweathermap.org/");
            string url = $"geo/1.0/direct?q={Uri.EscapeDataString(location)}&limit=5&appid=8c3a7a1fedef1ca1d1261de353d2698f";

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                geoLocationList = System.Text.Json.JsonSerializer.Deserialize<List<GeoLocation>>(json);
            }

            //call the weather API using the first geoLocation

            if (geoLocationList == null || geoLocationList.Count == 0)
            {
                throw new Exception("No location found for the specified query.");
            }
            var firstLocation = geoLocationList.FirstOrDefault();

            var weatherData = await GetWeatherByGeoLocation(firstLocation.Latitude, firstLocation.Longitude);
            
            // Set city name from the location parameter if it's not already set
            if (string.IsNullOrWhiteSpace(weatherData.CityName))
            {
                weatherData.CityName = location;
            }

            return weatherData;

        }

        private  Weather GetWeatherDataFromCurrentWeather(CurrentWeather currentWeather, string location, bool isRequireDisplayName = false)
        {
            var temp = currentWeather.Main?.Temp ?? 0;
            var icon = currentWeather.Weather?.FirstOrDefault()?.Icon ?? string.Empty;
            var IconUrl = $"https://openweathermap.org/img/wn/{icon}@2x.png";
            return new Weather
            {
                CityName = isRequireDisplayName ? currentWeather.Name :location,
                Temperature = currentWeather.Main?.Temp ?? 0,
                TempMax = currentWeather.Main?.TempMax ?? 0,
                TempMin = currentWeather.Main?.TempMin ?? 0,
                FeelsLike = currentWeather.Main?.FeelsLike ?? 0,
                Description = currentWeather.Weather?.FirstOrDefault()?.Description ?? string.Empty, // Example value
                Humidity = currentWeather.Main?.Humidity ?? 0,
                WindSpeed = currentWeather.Wind?.Speed ?? 0,
                Icon = IconUrl,
                IsLocationAvailable = currentWeather.IsLocationAvailable

            };

        }
        public async Task<Weather> GetWeatherAsync(string location)
        {
            //Check wheather the location is Latitude and Longitude or Postal code or City name
            if (string.IsNullOrWhiteSpace(location))
                return null;

            var cachedWeather = await _cacheService.GetAsync<Weather>(location);
            if (cachedWeather != null)
            {
                _logger.LogInformation("Cached data available for key: {Key}", location);
                return cachedWeather;
            }
            _logger.LogInformation("Cached data not available for key: {Key}. Get weather data from api", location);
            var isCordinates = WeatherHelper.IsLatLongPair(location, out double latitude, out double longitude);
            bool isLocationAvailable = false;
            bool isRequireDisplayName = false;
            string displayName = string.Empty;
            
            if (isCordinates)
            {
                // If coordinates are provided, skip geocoding and directly fetch weather
                isLocationAvailable = true;
                isRequireDisplayName = true;
            }
            else
            {
                isRequireDisplayName = WeatherHelper.IsPostalCode(location);
                (latitude, longitude, isLocationAvailable, displayName) = await GetCoordinates(location);
                isRequireDisplayName = isRequireDisplayName || WeatherHelper.IsPostalCode(location);
            }

            if (isLocationAvailable)
            {
                var weatherData = await GetWeatherByGeoLocation(latitude, longitude);
                weatherData.IsLocationAvailable = isLocationAvailable;
                
                // Set city name based on search type
                if (isCordinates)
                {
                    // For coordinates, use the API's returned city name
                    weatherData.CityName = weatherData.CityName;
                }
                else if (WeatherHelper.IsPostalCode(location))
                {
                    // For postal codes, use the displayName from geocoding
                    if (!string.IsNullOrWhiteSpace(displayName))
                    {
                        weatherData.CityName = displayName;
                    }
                }
                else
                {
                    // For city name searches, use the original search input with first letter capitalized
                    weatherData.CityName = char.ToUpper(location[0]) + location.Substring(1);
                }
                
                await _cacheService.SetAsync(location, weatherData, TimeSpan.FromMinutes(30));
                return weatherData;
            }

            _logger.LogInformation("Weather data not available for the key :{location}", location);
            return new Weather
            {
                CityName = location,
                Temperature = 0,
                TempMax = 0,
                TempMin = 0,
                FeelsLike = 0,
                Description = "Location not found. Please enter postal code or co-ordinates", // Example value
                Humidity = 0,
                WindSpeed = 0,
                Icon = string.Empty
            };
        }

        private async Task<Weather> GetWeatherByGeoLocation(double latitude, double longitude)
        {
            var currentWeather = new CurrentWeather();

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.openweathermap.org/");
                //https://api.openweathermap.org/data/2.5/weather?lat=12.9635&lon=77.4007&appid={{API key}}&units=metric&lang=en
                string url = $"data/2.5/weather?lat={latitude}&lon={longitude}&appid=8c3a7a1fedef1ca1d1261de353d2698f&units=metric&lang=en";

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    currentWeather = System.Text.Json.JsonSerializer.Deserialize<CurrentWeather>(json);
                }


            }
            catch (Exception ex)
            {
                
            }
            
            // Convert CurrentWeather to Weather object
            if (currentWeather != null && currentWeather.Main != null && currentWeather.Weather != null && currentWeather.Weather.Count > 0)
            {
                var icon = currentWeather.Weather.FirstOrDefault()?.Icon ?? string.Empty;
                var iconUrl = $"https://openweathermap.org/img/wn/{icon}@2x.png";
                
                return new Weather
                {
                    CityName = currentWeather.Name ?? string.Empty,
                    Temperature = currentWeather.Main.Temp,
                    TempMax = currentWeather.Main.TempMax,
                    TempMin = currentWeather.Main.TempMin,
                    FeelsLike = currentWeather.Main.FeelsLike,
                    Description = currentWeather.Weather.FirstOrDefault()?.Description ?? string.Empty,
                    Humidity = currentWeather.Main.Humidity,
                    WindSpeed = currentWeather.Wind?.Speed ?? 0.0,
                    Icon = iconUrl,
                    IsLocationAvailable = true
                };
            }
            
            return new Weather
            {
                CityName = string.Empty,
                Temperature = 0,
                TempMax = 0,
                TempMin = 0,
                FeelsLike = 0,
                Description = "Weather data not available",
                Humidity = 0,
                WindSpeed = 0,
                Icon = string.Empty,
                IsLocationAvailable = false
            };
        }

        public async Task<(double Latitude, double Longitude, bool IsLocationAvailabe, string DisplayName)> GetCoordinates(string location)
        {
            var islocationAvailable = false;

            // Use OpenWeatherMap geocoding API for more accurate location results
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.openweathermap.org/");
            
            // Use OpenWeatherMap geocoding API with limit and country filter for better accuracy
            string url = $"geo/1.0/direct?q={Uri.EscapeDataString(location)}&limit=5&appid=8c3a7a1fedef1ca1d1261de353d2698f";

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var locationData = System.Text.Json.JsonSerializer.Deserialize<List<GeoLocation>>(json);
                
                if (locationData != null && locationData.Count > 0)
                {
                    // Try to find the most relevant match
                    var bestMatch = locationData.FirstOrDefault();
                    
                    if (bestMatch != null)
                    {
                        islocationAvailable = true;
                        var displayName = !string.IsNullOrEmpty(bestMatch.State) 
                            ? $"{bestMatch.Name}, {bestMatch.State}, {bestMatch.Country}"
                            : $"{bestMatch.Name}, {bestMatch.Country}";
                            
                        return (bestMatch.Latitude, bestMatch.Longitude, islocationAvailable, displayName);
                    }
                }
            }

            return (0, 0, islocationAvailable, string.Empty);

        }
    }
}
