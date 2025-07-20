using WeatherApp.IService;
using WeatherApp.Models;

namespace WeatherApp.Service
{
    public class WeatherService : IWeatherService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public WeatherService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ;
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

            var temp = currentWeather.Main?.Temp ?? 0;

            var icon = currentWeather.Weather?.FirstOrDefault()?.Icon ?? string.Empty;
            var IconUrl = $"https://openweathermap.org/img/wn/{icon}@2x.png";
            return new Weather
            {
                CityName = location,
                Temperature = currentWeather.Main?.Temp ?? 0,
                TempMax = currentWeather.Main?.TempMax ?? 0,
                TempMin = currentWeather.Main?.TempMin ?? 0,
                FeelsLike = currentWeather.Main?.FeelsLike ?? 0,
                Description = currentWeather.Weather?.FirstOrDefault()?.Description ?? string.Empty, // Example value
                Humidity = currentWeather.Main?.Humidity ?? 0,
                WindSpeed = currentWeather.Wind?.Speed ?? 0,
                Icon = IconUrl
            };

        }

        private async Task<CurrentWeather> GetWeatherByGeoLocation(decimal latitude, decimal longitude)
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
    }
}
