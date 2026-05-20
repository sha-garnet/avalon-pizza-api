using AvalonPizza.Server.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AvalonPizza.Server.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;

    // Configures the serializer to look for camelCase and ignore casing during deserialization
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Get from Redis storage
    /// </summary>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            // Attempt to retrieve from Redis
            var jsonData = await _cache.GetStringAsync(key);

            if (string.IsNullOrWhiteSpace(jsonData))
                return default;

            return JsonSerializer.Deserialize<T>(jsonData, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize cache for key: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error while retrieving key: {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Set the cache in Redis storage
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null) where T : class
    {
        if (value == null) return;

        try
        {
            // Serialize the object for Redis storage
            var jsonData = JsonSerializer.Serialize(value, JsonOptions);

            // The Sliding Expiration must always be shorter than the Absolute Expiration
            var resolvedAbsolute = absoluteExpiration ?? TimeSpan.FromHours(12);
            var resolvedSliding = resolvedAbsolute.TotalHours > 4
                ? TimeSpan.FromHours(4)
                : TimeSpan.FromMinutes(resolvedAbsolute.TotalMinutes / 2); // Dynamic fallback

            // Configure Cache Policy
            var options = new DistributedCacheEntryOptions
            {
                // setting TTL (Time To Live)
                AbsoluteExpirationRelativeToNow = resolvedAbsolute,
                SlidingExpiration = resolvedSliding
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