using AvalonPizza.Server.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AvalonPizza.Server.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        // Attempt to retrieve from Redis
        var jsonData = await _cache.GetStringAsync(key);
 
        if (string.IsNullOrWhiteSpace(jsonData))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(jsonData);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null) where T : class
    {
        if (value == null) return;

        try
        {
            // Serialize the object for Redis storage
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set cache for key: {Key}", key);
        }
    }

    /// <summary>
    /// Cache Invalidation, delete the cache entry whenever a Create, Update, or Delete operation occurs
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove cache key: {Key}", key);
        }
    }
}