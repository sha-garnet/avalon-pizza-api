using AvalonPizza.Server.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AvalonPizza.Server.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        // // Attempt to retrieve from Redis
        var jsonData = await _cache.GetStringAsync(key);
        return jsonData == null ? default : JsonSerializer.Deserialize<T>(jsonData);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null)
    {
        // Serialize the list for Redis storage
        var jsonData = JsonSerializer.Serialize(value);

        // Configure Cache Policy
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromHours(12),
            SlidingExpiration = TimeSpan.FromHours(4)
        };
        
        // Save to Redis
        await _cache.SetStringAsync(key, jsonData, options);
    }
}