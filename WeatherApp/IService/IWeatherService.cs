using WeatherApp.Models;

namespace WeatherApp.IService
{
    public interface IWeatherService
    {
        Task<Weather> GetWeatherData(string location);
    }
}
