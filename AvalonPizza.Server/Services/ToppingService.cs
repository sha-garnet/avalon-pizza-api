using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Services;

public class ToppingService : IToppingService
{
    private readonly IToppingRepository _toppingRepository;
    private readonly ICacheService _cacheService;
    private const string ToppingsCacheKey = "ToppingService_AllToppings";

    public ToppingService(IToppingRepository toppingRepository, ICacheService cacheService)
    {
        _toppingRepository = toppingRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<Topping>> GetAllToppingsAsync()
    {
        // Attempt to retrieve from Redis
        var toppings = await _cacheService.GetAsync<IEnumerable<Topping>>(ToppingsCacheKey);

        if (toppings == null)
        {
            // Cache Miss: Fetch from SQL Server via Repository
            toppings = await _toppingRepository.GetAllToppingsAsync();
            // Save to Redis
            await _cacheService.SetAsync(ToppingsCacheKey, toppings);
        }

        return toppings;
    }
}
