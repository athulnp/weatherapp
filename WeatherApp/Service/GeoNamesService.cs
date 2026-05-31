using System.Text.Json;

namespace WeatherApp.Service
{
    public class CountryCitiesService
    {
        private readonly HttpClient _httpClient;

        public CountryCitiesService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<GeoCity>> GetPopularCitiesByCountry(string countryCode)
        {
            try
            {
                var url = $"https://restcountries.com/v3.1/alpha/{countryCode}";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    return new List<GeoCity>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<List<RestCountryResponse>>(json);
                
                if (data == null || data.Count == 0)
                {
                    return new List<GeoCity>();
                }

                var country = data[0];
                var cities = new List<GeoCity>();

                // Add capital city
                if (country.Capital != null && country.Capital.Count > 0)
                {
                    cities.Add(new GeoCity { Name = country.Capital[0], CountryCode = country.Cca2 });
                }

                // REST Countries doesn't provide other major cities, so we'll use the fallback
                return cities;
            }
            catch (Exception ex)
            {
                return new List<GeoCity>();
            }
        }

        public List<GeoCity> GetFallbackCities(string countryCode)
        {
            // Predefined cities for major countries - no API needed
            var fallbackMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["IN"] = ["Delhi", "Mumbai", "Bangalore", "Chennai", "Hyderabad", "Kochi"],
                ["GB"] = ["London", "Manchester", "Birmingham", "Glasgow", "Liverpool", "Leeds"],
                ["US"] = ["New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia"],
                ["AU"] = ["Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide", "Gold Coast"],
                ["CA"] = ["Toronto", "Vancouver", "Montreal", "Calgary", "Ottawa", "Edmonton"],
                ["DE"] = ["Berlin", "Munich", "Hamburg", "Cologne", "Frankfurt", "Stuttgart"],
                ["FR"] = ["Paris", "Marseille", "Lyon", "Toulouse", "Nice", "Nantes"],
                ["JP"] = ["Tokyo", "Osaka", "Kyoto", "Yokohama", "Nagoya", "Sapporo"],
                ["IT"] = ["Rome", "Milan", "Naples", "Turin", "Palermo", "Genoa"],
                ["ES"] = ["Madrid", "Barcelona", "Valencia", "Seville", "Bilbao", "Malaga"],
                ["BR"] = ["São Paulo", "Rio de Janeiro", "Brasília", "Salvador", "Fortaleza", "Belo Horizonte"],
                ["RU"] = ["Moscow", "Saint Petersburg", "Novosibirsk", "Yekaterinburg", "Kazan", "Nizhny Novgorod"],
                ["CN"] = ["Beijing", "Shanghai", "Guangzhou", "Shenzhen", "Chengdu", "Hangzhou"],
                ["ZA"] = ["Johannesburg", "Cape Town", "Durban", "Pretoria", "Port Elizabeth", "Bloemfontein"],
                ["MX"] = ["Mexico City", "Guadalajara", "Monterrey", "Puebla", "Tijuana", "León"],
                ["KR"] = ["Seoul", "Busan", "Incheon", "Daegu", "Daejeon", "Gwangju"],
                ["NL"] = ["Amsterdam", "Rotterdam", "The Hague", "Utrecht", "Eindhoven", "Groningen"],
                ["SG"] = ["Singapore"],
                ["AE"] = ["Dubai", "Abu Dhabi", "Sharjah", "Ajman", "Ras Al Khaimah", "Fujairah"],
                ["TR"] = ["Istanbul", "Ankara", "Izmir", "Bursa", "Antalya", "Adana"],
                ["TH"] = ["Bangkok", "Chiang Mai", "Pattaya", "Phuket", "Nonthaburi", "Hat Yai"],
                ["MY"] = ["Kuala Lumpur", "George Town", "Ipoh", "Shah Alam", "Petaling Jaya", "Johor Bahru"],
                ["ID"] = ["Jakarta", "Surabaya", "Bandung", "Medan", "Semarang", "Makassar"],
                ["PH"] = ["Manila", "Quezon City", "Davao", "Cebu City", "Caloocan", "Zamboanga"],
                ["VN"] = ["Ho Chi Minh City", "Hanoi", "Hai Phong", "Da Nang", "Can Tho", "Bien Hoa"],
                ["EG"] = ["Cairo", "Alexandria", "Giza", "Shubra El-Kheima", "Port Said", "Luxor"],
                ["SA"] = ["Riyadh", "Jeddah", "Mecca", "Medina", "Dammam", "Khobar"]
            };

            if (fallbackMap.TryGetValue(countryCode, out var cities))
            {
                return cities.Select(name => new GeoCity { Name = name }).ToList();
            }

            return new List<GeoCity>();
        }
    }

    public class RestCountryResponse
    {
        public string Cca2 { get; set; } = string.Empty;
        public List<string>? Capital { get; set; }
    }

    public class GeoCity
    {
        public string Name { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public int? Population { get; set; }
    }
}
