using WeatherApp.Models;

namespace WeatherApp.IService
{
    public interface IWeatherService
    {
        Task<Weather> GetWeatherData(string location);
        Task<Weather> GetWeatherAsync(string location);
        Task<(double Latitude, double Longitude, bool IsLocationAvailabe)> GetCoordinates(string location);
    }
}
