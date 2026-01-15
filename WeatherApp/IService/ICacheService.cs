using System.Threading.Tasks;

namespace WeatherApp.IService
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        /*Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);*/
        Task UpdateAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
    }
}