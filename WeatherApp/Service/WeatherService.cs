using Newtonsoft.Json;
using NuGet.Packaging.Licenses;
using System.Text.RegularExpressions;
using WeatherApp.IService;
using WeatherApp.Models;
using WeatherApp.Helper;

namespace WeatherApp.Service
{
    public class WeatherService : IWeatherService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public WeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ;
            _configuration = configuration;
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

            var currentWeather = await GetWeatherByGeoLocation(firstLocation.Latitude, firstLocation.Longitude);

            return GetWeatherDataFromCurrentWeather(currentWeather, location);

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

            var isCordinates = WeatherHelper.IsLatLongPair(location, out double latitude, out double longitude);
            bool isLocationAvailable = false;
            bool isRequireDisplayName = false;
            string displayName = string.Empty;
            isRequireDisplayName = WeatherHelper.IsPostalCode(location);
            (latitude, longitude, isLocationAvailable, displayName) = await GetCoordinates(location);
            isRequireDisplayName = isCordinates || WeatherHelper.IsPostalCode(location);
            //if (!isCordinates)
            //{
            //    isRequireDisplayName = WeatherHelper.IsPostalCode(location);
            //    (latitude, longitude, isLocationAvailable, displayName) = await GetCoordinates(location);
            //}
            //else
            //{ 
            //    //var coords = location.Split(',', StringSplitOptions.RemoveEmptyEntries);
            //    //bool latAvailable = WeatherHelper.TryGetLatitude(coords[0], out latitude);
            //    //bool longAvailabe = WeatherHelper.TryGetLatitude(coords[1], out longitude);
            //    isLocationAvailable = isCordinates;
            //    isRequireDisplayName = true;
            //}

            if (isLocationAvailable)
            {
                var currentWeather = await GetWeatherByGeoLocation(latitude, longitude);
                currentWeather.IsLocationAvailable = isLocationAvailable;
                currentWeather.Name = isRequireDisplayName && !string.IsNullOrWhiteSpace(displayName) ? displayName : currentWeather.Name;
                return GetWeatherDataFromCurrentWeather(currentWeather, location, isRequireDisplayName);
            }
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

        private async Task<CurrentWeather> GetWeatherByGeoLocation(double latitude, double longitude)
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
            return currentWeather ?? new CurrentWeather();
        }

        public async Task<(double Latitude, double Longitude, bool IsLocationAvailabe, string DisplayName)> GetCoordinates(string location)
        {
            var islocationAvailable = false;

            //Get the coordinates of the location from nominatim API
            var baseUrl = _configuration["GeoCodingApis:Nominatim:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                var qSearch = _configuration["GeoCodingApis:Nominatim:qsearch"];
                qSearch = qSearch != null ? string.Format(qSearch,location) : qSearch;
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add(
                                                "User-Agent",
                                                "User-Agent (athulonline13@gmail.com)");

                HttpResponseMessage response = await client.GetAsync(qSearch);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var locationData = JsonConvert.DeserializeObject<List<NominatimLocation>>(json);
                    var defaultLocation = locationData?.FirstOrDefault();

                    if (defaultLocation != null)
                    {
                        islocationAvailable = true;
                        return (Convert.ToDouble(defaultLocation.Latitude), Convert.ToDouble(defaultLocation.Longitude), islocationAvailable, defaultLocation.DisplayName);
                    }
                }
            }

            return (0, 0, islocationAvailable, string.Empty);

        }
    }
}
