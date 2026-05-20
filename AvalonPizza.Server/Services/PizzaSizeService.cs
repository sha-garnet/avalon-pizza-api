using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Services;

public class PizzaSizeService : IPizzaSizeService
{
    private readonly IPizzaSizeRepository _pizzaSizeRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<PizzaSizeService> _logger;
    private const string SizesCacheKey = "pizza:sizes:all";

    public PizzaSizeService(
        IPizzaSizeRepository sizeRepository,
        ICacheService cacheService,
        ILogger<PizzaSizeService> logger)
    {
        _pizzaSizeRepository = sizeRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<PizzaSize>> GetAllSizesAsync()
    {
        try
        {
            // Attempt to retrieve from Redis
            var cachedSizes = await _cacheService.GetAsync<IEnumerable<PizzaSize>>(SizesCacheKey);

            if (cachedSizes != null)
            {
                _logger.LogInformation("Cache Hit: Retrieved pizza sizes from Redis.");
                return cachedSizes;
            }

            // Cache Miss - fetching from data store
            _logger.LogWarning("Cache Miss: Fetching pizza sizes from data store");
            var sizes = await _pizzaSizeRepository.GetAllAsync();

            // Save to Redis
            if (sizes != null && sizes.Any())
            {
                await _cacheService.SetAsync(SizesCacheKey, sizes);
            }

            return sizes;
        }
        catch (Exception ex)
        {
            // If Redis fails "Fail Over" to the data store
            _logger.LogError(ex, "Redis failure in PizzaSizeService. Falling back to data store.");
            return await _pizzaSizeRepository.GetAllAsync();
        }
    }
}