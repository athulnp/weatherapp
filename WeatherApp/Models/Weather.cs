using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WeatherApp.Models
{
    public class Weather
    {
        public string CityName { get; set; }
        public double Temperature { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public double FeelsLike { get; set; }
        public string Description { get; set; }
        public int Humidity { get; set; } 
        public double WindSpeed { get; set; }
        public string Icon { get; set; }
        public bool IsLocationAvailable { get; set; }
    }
}
