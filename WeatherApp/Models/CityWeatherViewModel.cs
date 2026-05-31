using WeatherApp.Service;

namespace WeatherApp.Models
{
    public class CityWeatherViewModel
    {
        public Weather WeatherData { get; set; }
        public dynamic ForecastData { get; set; }
        public string CityName { get; set; }
        public string PageTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string CanonicalUrl { get; set; }
        public List<CitySearchResult>? PopularCities { get; set; }
    }
}
