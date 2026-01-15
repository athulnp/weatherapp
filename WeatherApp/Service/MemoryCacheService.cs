namespace WeatherApp.Service
{
    using Microsoft.Extensions.Caching.Memory;
    using WeatherApp.IService;

    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheService> _logger;

        public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key)
        {
           if( _cache.TryGetValue(key, out T? value))
            {
                _logger.LogInformation("Cache GET for key: {Key}", key);
                return Task.FromResult(value);
            }
            _logger.LogInformation("Cache MISS for key: {Key}", key);
            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = CreateOptions(expiry);
            _cache.Set(key, value, options);
            _logger.LogInformation("Cache SET for key: {Key}", key);
            return Task.CompletedTask;
        }

        //public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        //{
        //    if (_cache.TryGetValue(key, out T? cached))
        //        return cached;

        //    var value = await factory();
        //    await SetAsync(key, value, expiry);
        //    return value;
        //}

        public Task UpdateAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            _cache.Remove(key);
            SetAsync(key, value, expiry);
            _logger.LogInformation("Cache UPDATED for key: {Key}", key);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            _logger.LogInformation("Cache REMOVED for key: {Key}", key);
            return Task.CompletedTask;
        }

        private static MemoryCacheEntryOptions CreateOptions(TimeSpan? expiry)
        {
            return new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(30),
                Priority = CacheItemPriority.Normal
            };
        }
    }

}
