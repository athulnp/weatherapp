using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WeatherApp.Models
{
    public class Weather
    {
        public string CityName { get; set; } = "New York";
        public double Temperature { get; set; } = 28.5;
        public string Description { get; set; } = "Partly Cloudy";
        public int Humidity { get; set; } = 65;
        public double WindSpeed { get; set; } = 14.2;
    }
}
