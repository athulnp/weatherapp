using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NuGet.Packaging.Licenses;
using System.Text.Json;
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
        private Dictionary<string, (double lat, double lon, string name)> _postalCodeCache;

        public WeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ICacheService cacheService, ILogger<WeatherService> logger)
        {
            _httpClientFactory = httpClientFactory ;
            _configuration = configuration;
            _cacheService = cacheService;
            _logger = logger;
            _postalCodeCache = new Dictionary<string, (double, double, string)>();
            _ = LoadPostalCodesFromFile(); // Fire and forget
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
                
                await _cacheService.SetAsync(location, weatherData, TimeSpan.FromMinutes(20));
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
                var countryCode = currentWeather.Sys?.Country ?? string.Empty;
                
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
                    IsLocationAvailable = true,
                    Latitude = latitude,
                    Longitude = longitude,
                    CountryCode = countryCode
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

            // Check if location is a postal code (numeric, 5-6 digits)
            if (WeatherHelper.IsPostalCode(location))
            {
                // For postal codes, use OpenWeatherMap ZIP API
                return await GetCoordinatesFromOpenWeatherMapZip(location);
            }
            else
            {
                // For city names, use OpenWeatherMap geocoding
                return await GetCoordinatesFromOpenWeatherMap(location);
            }
        }

        private async Task<(double Latitude, double Longitude, bool IsLocationAvailabe, string DisplayName)> GetCoordinatesFromOpenWeatherMapZip(string postalCode)
        {
            try
            {
                // Try Nominatim first for postal codes (better accuracy for India)
                var nominatimResult = await GetCoordinatesFromNominatimPostalCode(postalCode);
                if (nominatimResult.IsLocationAvailabe)
                {
                    return nominatimResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nominatim postal code search failed, trying fallback for postal code: {postalCode}", postalCode);
            }
            
            try
            {
                // Try PostalPincode API (Indian postal codes)
                var postalCodeApiResult = await GetCoordinatesFromPostalCodeAPI(postalCode);
                if (postalCodeApiResult.IsLocationAvailabe)
                {
                    return postalCodeApiResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PostalPincode API failed, trying fallback for postal code: {postalCode}", postalCode);
            }
            
            // Fallback to OpenWeatherMap ZIP API
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.openweathermap.org/");
                
                // Use OpenWeatherMap ZIP API with India country code
                string url = $"geo/1.0/zip?zip={Uri.EscapeDataString(postalCode)},IN&appid=8c3a7a1fedef1ca1d1261de353d2698f";
                
                HttpResponseMessage response = await client.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var zipData = System.Text.Json.JsonSerializer.Deserialize<GeoLocation>(json);
                    
                    if (zipData != null && zipData.Latitude != 0 && zipData.Longitude != 0)
                    {
                        // Use reverse geocoding to get exact location name
                        var exactLocationName = await GetExactLocationNameFromOpenWeatherMap(zipData.Latitude, zipData.Longitude);
                        
                        var displayName = !string.IsNullOrEmpty(exactLocationName) 
                            ? exactLocationName
                            : (!string.IsNullOrEmpty(zipData.State) 
                                ? $"{zipData.Name}, {zipData.State}, {zipData.Country}"
                                : $"{zipData.Name}, {zipData.Country}");
                        
                        return (zipData.Latitude, zipData.Longitude, true, displayName);
                    }
                }
                else
                {
                    _logger.LogError("OpenWeatherMap ZIP API returned status code: {statusCode} for postal code: {postalCode}", response.StatusCode, postalCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coordinates from OpenWeatherMap ZIP API for postal code: {postalCode}", postalCode);
            }
            
            return (0, 0, false, string.Empty);
        }

        private async Task<(double Latitude, double Longitude, bool IsLocationAvailabe, string DisplayName)> GetCoordinatesFromNominatimPostalCode(string postalCode)
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    // Disable SSL certificate validation
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    
                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        
                        // Add required User-Agent header for Nominatim
                        client.DefaultRequestHeaders.Add("User-Agent", "WeatherApp/1.0 (contact: info@weatherapp.com)");
                        
                        // Search for postal code in India using Nominatim
                        string url = $"https://nominatim.openstreetmap.org/search?postalcode={Uri.EscapeDataString(postalCode)}&country=India&format=json&limit=1";
                        
                        HttpResponseMessage response = await client.GetAsync(url);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            var locationData = System.Text.Json.JsonSerializer.Deserialize<List<NominatimLocation>>(json);
                            
                            if (locationData != null && locationData.Count > 0)
                            {
                                var location = locationData.FirstOrDefault();
                                if (location != null && !string.IsNullOrEmpty(location.Latitude) && !string.IsNullOrEmpty(location.Longitude))
                                {
                                    if (double.TryParse(location.Latitude, out double lat) && double.TryParse(location.Longitude, out double lon))
                                    {
                                        var displayName = location.DisplayName ?? $"{location.Name}, India";
                                        return (lat, lon, true, displayName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting coordinates from Nominatim for postal code: {postalCode}", postalCode);
            }
            
            return (0, 0, false, string.Empty);
        }

        private async Task<(double Latitude, double Longitude, bool IsLocationAvailabe, string DisplayName)> GetCoordinatesFromPostalCodeAPI(string postalCode)
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    // Disable SSL certificate validation
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    
                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        
                        // Use PostalPincode API - comprehensive postal code database for India
                        // Uses HTTP (not HTTPS) - free, no API key required
                        string url = $"http://api.postalpincode.in/postoffice/{Uri.EscapeDataString(postalCode)}";
                        
                        HttpResponseMessage response = await client.GetAsync(url);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            
                            using (var doc = System.Text.Json.JsonDocument.Parse(json))
                            {
                                var root = doc.RootElement;
                                
                                // PostalPincode API returns an array
                                if (root.ValueKind == System.Text.Json.JsonValueKind.Array && root.GetArrayLength() > 0)
                                {
                                    var firstResult = root[0];
                                    
                                    if (firstResult.TryGetProperty("Status", out var status) && status.GetString() == "Success")
                                    {
                                        if (firstResult.TryGetProperty("PostOffice", out var postOffices) && postOffices.GetArrayLength() > 0)
                                        {
                                            var postOffice = postOffices[0];
                                            
                                            if (postOffice.TryGetProperty("Name", out var name) &&
                                                postOffice.TryGetProperty("District", out var district) &&
                                                postOffice.TryGetProperty("State", out var state))
                                            {
                                                var placeName = name.GetString() ?? "";
                                                var districtName = district.GetString() ?? "";
                                                var stateName = state.GetString() ?? "";
                                                
                                                // Get coordinates using reverse geocoding
                                                var displayName = $"{placeName}, {districtName}, {stateName}, India";
                                                
                                                // Use OpenWeatherMap reverse geocoding to get lat/lon from place name
                                                var coordinates = await GetCoordinatesFromPlaceName(displayName);
                                                if (coordinates.IsLocationAvailabe)
                                                {
                                                    return coordinates;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogError("PostalPincode API returned status code: {statusCode} for postal code: {postalCode}", response.StatusCode, postalCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coordinates from PostalPincode API for postal code: {postalCode}", postalCode);
            }
            
            return (0, 0, false, string.Empty);
        }

        private async Task<(double Latitude, double Longitude, bool IsLocationAvailabe, string DisplayName)> GetCoordinatesFromPlaceName(string placeName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.openweathermap.org/");
                
                string url = $"geo/1.0/direct?q={Uri.EscapeDataString(placeName)}&limit=1&appid=8c3a7a1fedef1ca1d1261de353d2698f";
                
                HttpResponseMessage response = await client.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var locationData = System.Text.Json.JsonSerializer.Deserialize<List<GeoLocation>>(json);
                    
                    if (locationData != null && locationData.Count > 0)
                    {
                        var location = locationData.FirstOrDefault();
                        if (location != null)
                        {
                            return (location.Latitude, location.Longitude, true, placeName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting coordinates from place name: {placeName}. Will use fallback.", placeName);
                // Return empty to trigger fallback
            }
            
            return (0, 0, false, string.Empty);
        }

        private async Task LoadPostalCodesFromFile()
        {
            // No longer needed - using APIs instead
            return;
        }

        private (double Latitude, double Longitude, bool IsLocationAvailabe, string DisplayName) GetKnownPostalCodeLocation(string postalCode)
        {
            // No longer used - using APIs for all postal codes
            return (0, 0, false, string.Empty);
        }

        private async Task<string> GetExactLocationNameFromOpenWeatherMap(double latitude, double longitude)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.openweathermap.org/");
                
                // Use reverse geocoding to get exact location details
                string url = $"geo/1.0/reverse?lat={latitude}&lon={longitude}&limit=1&appid=8c3a7a1fedef1ca1d1261de353d2698f";
                
                HttpResponseMessage response = await client.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var locationData = System.Text.Json.JsonSerializer.Deserialize<List<GeoLocation>>(json);
                    
                    if (locationData != null && locationData.Count > 0)
                    {
                        var location = locationData.FirstOrDefault();
                        if (location != null)
                        {
                            return !string.IsNullOrEmpty(location.State)
                                ? $"{location.Name}, {location.State}, {location.Country}"
                                : $"{location.Name}, {location.Country}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exact location name from OpenWeatherMap reverse geocoding");
            }
            
            return string.Empty;
        }

        private async Task<(double Latitude, double Longitude, bool IsLocationAvailabe, string DisplayName)> GetCoordinatesFromOpenWeatherMap(string location)
        {
            var islocationAvailable = false;

            // Try Nominatim first (better for villages and small places)
            var nominatimResult = await GetCoordinatesFromNominatim(location);
            if (nominatimResult.IsLocationAvailabe)
            {
                return nominatimResult;
            }

            // Fallback to OpenWeatherMap geocoding API for city names
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.openweathermap.org/");
            
            string url = $"geo/1.0/direct?q={Uri.EscapeDataString(location)}&limit=5&appid=8c3a7a1fedef1ca1d1261de353d2698f";

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var locationData = System.Text.Json.JsonSerializer.Deserialize<List<GeoLocation>>(json);
                
                if (locationData != null && locationData.Count > 0)
                {
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

        private async Task<(double Latitude, double Longitude, bool IsLocationAvailabe, string DisplayName)> GetCoordinatesFromNominatim(string location)
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    // Disable SSL certificate validation
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    
                    using (var client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        
                        // Add required User-Agent header for Nominatim
                        client.DefaultRequestHeaders.Add("User-Agent", "WeatherApp/1.0 (contact: info@weatherapp.com)");
                        
                        // Search for place in India using Nominatim
                        string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(location)},India&format=json&limit=1";
                        
                        HttpResponseMessage response = await client.GetAsync(url);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            var locationData = System.Text.Json.JsonSerializer.Deserialize<List<NominatimLocation>>(json);
                            
                            if (locationData != null && locationData.Count > 0)
                            {
                                var location_item = locationData.FirstOrDefault();
                                if (location_item != null && !string.IsNullOrEmpty(location_item.Latitude) && !string.IsNullOrEmpty(location_item.Longitude))
                                {
                                    if (double.TryParse(location_item.Latitude, out double lat) && double.TryParse(location_item.Longitude, out double lon))
                                    {
                                        var displayName = location_item.DisplayName ?? $"{location_item.Name}, India";
                                        return (lat, lon, true, displayName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nominatim API failed for location: {location}. Trying fallback.", location);
            }
            
            return (0, 0, false, string.Empty);
        }

        public async Task<List<DailyForecast>> GetWeatherForecast(double latitude, double longitude)
        {
            try
            {
                // Create cache key for forecast
                string cacheKey = $"forecast_{latitude}_{longitude}";
                
                // Check cache first
                var cachedForecast = await _cacheService.GetAsync<List<DailyForecast>>(cacheKey);
                if (cachedForecast != null)
                {
                    _logger.LogInformation("Cached forecast data available for coordinates: {lat}, {lon}", latitude, longitude);
                    return cachedForecast;
                }
                
                var client = _httpClientFactory.CreateClient("weatherClient");
                // Use 5-day forecast API (free tier)
                string url = $"data/2.5/forecast?lat={latitude}&lon={longitude}&appid=8c3a7a1fedef1ca1d1261de353d2698f&units=metric";

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var forecastData = System.Text.Json.JsonSerializer.Deserialize<FiveDayForecastResponse>(json);
                    
                    if (forecastData?.List != null && forecastData.List.Count > 0)
                    {
                        // Group forecast items by date and calculate daily max/min
                        var dailyForecasts = forecastData.List
                            .GroupBy(item => DateTimeOffset.FromUnixTimeSeconds(item.Dt).DateTime.Date)
                            .Select(group => new DailyForecast
                            {
                                Dt = new DateTimeOffset(group.Key).ToUnixTimeSeconds(),
                                Temp = new DailyTemperature
                                {
                                    Day = group.Average(item => item.Main.Temp),
                                    Min = group.Min(item => item.Main.TempMin),
                                    Max = group.Max(item => item.Main.TempMax),
                                    Night = group.Where(item => item.DtTxt.Contains("00:00:00") || item.DtTxt.Contains("03:00:00")).Select(item => item.Main.Temp).DefaultIfEmpty(group.Average(item => item.Main.Temp)).Average(),
                                    Eve = group.Where(item => item.DtTxt.Contains("18:00:00")).Select(item => item.Main.Temp).DefaultIfEmpty(group.Average(item => item.Main.Temp)).Average(),
                                    Morn = group.Where(item => item.DtTxt.Contains("09:00:00") || item.DtTxt.Contains("12:00:00")).Select(item => item.Main.Temp).DefaultIfEmpty(group.Average(item => item.Main.Temp)).Average()
                                },
                                FeelsLike = new DailyFeelsLike
                                {
                                    Day = group.Average(item => item.Main.FeelsLike),
                                    Night = group.Where(item => item.DtTxt.Contains("00:00:00") || item.DtTxt.Contains("03:00:00")).Select(item => item.Main.FeelsLike).DefaultIfEmpty(group.Average(item => item.Main.FeelsLike)).Average(),
                                    Eve = group.Where(item => item.DtTxt.Contains("18:00:00")).Select(item => item.Main.FeelsLike).DefaultIfEmpty(group.Average(item => item.Main.FeelsLike)).Average(),
                                    Morn = group.Where(item => item.DtTxt.Contains("09:00:00") || item.DtTxt.Contains("12:00:00")).Select(item => item.Main.FeelsLike).DefaultIfEmpty(group.Average(item => item.Main.FeelsLike)).Average()
                                },
                                Humidity = (int)group.Average(item => item.Main.Humidity),
                                WindSpeed = group.Average(item => item.Wind?.Speed ?? 0),
                                Weather = group.First().Weather
                            })
                            .Take(5)
                            .ToList();
                        
                        // Cache the forecast data for 2 hours (forecasts change less frequently)
                        await _cacheService.SetAsync(cacheKey, dailyForecasts, TimeSpan.FromHours(2));
                        _logger.LogInformation("Forecast data cached for coordinates: {lat}, {lon}", latitude, longitude);
                        
                        return dailyForecasts;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get weather forecast for coordinates: {lat}, {lon}", latitude, longitude);
            }

            return new List<DailyForecast>();
        }
    }
}
