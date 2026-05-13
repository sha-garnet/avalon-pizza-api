using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace AvalonPizza.Server.Services;

public class PizzaSizeService : IPizzaSizeService
{
    private readonly IPizzaSizeRepository _pizzaSizeRepository;
    private readonly ICacheService _cacheService;
    private const string SizesCacheKey = "PizzaSizes_List";

    public PizzaSizeService(IPizzaSizeRepository sizeRepository, ICacheService cacheService)
    {
        _pizzaSizeRepository = sizeRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<PizzaSize>> GetAllSizesAsync()
    {
        // Attempt to retrieve from Redis
        var sizes = await _cacheService.GetAsync<IEnumerable<PizzaSize>>(SizesCacheKey);

        if (sizes == null)
        {
            // Cache Miss: Fetch from SQL Server via Repository
            sizes = await _pizzaSizeRepository.GetAllAsync();
            // Save to Redis
            await _cacheService.SetAsync(SizesCacheKey, sizes);
        }

        return sizes;
    }

}