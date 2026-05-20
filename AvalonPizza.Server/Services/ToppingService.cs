using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Services;

public class ToppingService : IToppingService
{
    private readonly IToppingRepository _toppingRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ToppingService> _logger;
    private const string ToppingsCacheKey = "pizza:toppings:all";

    public ToppingService(
        IToppingRepository toppingRepository,
        ICacheService cacheService,
        ILogger<ToppingService> logger)
    {
        _toppingRepository = toppingRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<Topping>> GetAllToppingsAsync()
    {
        try
        {
            // Attempt to retrieve from Redis
            var cachedToppings = await _cacheService.GetAsync<IEnumerable<Topping>>(ToppingsCacheKey);

            if (cachedToppings != null)
            {
                _logger.LogInformation("Cache Hit: Retrieved pizza toppings from Redis.");
                return cachedToppings;
            }

            // Cache Miss - fetching from data store
            _logger.LogWarning("Cache Miss: Fetching pizza toppings from data store");
            var toppings = await _toppingRepository.GetAllToppingsAsync();

            // Save to Redis
            if (toppings != null && toppings.Any())
            {
                await _cacheService.SetAsync(ToppingsCacheKey, toppings);
            }

            return toppings;
        }
        catch (Exception ex)
        {
            // If Redis fails "Fail Over" to the data store
            _logger.LogError(ex, "Redis failure in ToppingService. Falling back to data store.");
            return await _toppingRepository.GetAllToppingsAsync();
        }
    }
}
