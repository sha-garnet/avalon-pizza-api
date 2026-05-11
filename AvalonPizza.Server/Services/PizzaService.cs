using AvalonPizza.Server.Controllers;
using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Services;

public class PizzaService : IPizzaService
{
    private readonly IPizzaRepository _pizzaRepo;
    private readonly ILogger<PizzaController> _logger;

    public PizzaService(
        IPizzaRepository pizzaRepo,
        ILogger<PizzaController> logger)
    {
        _pizzaRepo = pizzaRepo;
        _logger = logger;
    }

    public async Task<PizzaOrder> ProcessOrderAsync(PizzaOrder order)
    {
        decimal basePrice = order.Size switch
        {
            "Small" => 8.00m,
            "Medium" => 10.00m,
            "Large" => 12.00m,
            _ => throw new ArgumentException("Invalid pizza size")
        };

        decimal toppingPrice = order.Toppings.Count * 1.50m;
        order.Price = basePrice + toppingPrice;

        await _pizzaRepo.AddAsync(order);

        return order;
    }

    public async Task<IEnumerable<object>> GetAllOrdersAsync()
    {
        // Maybe we want to log who is looking at the data
        _logger.LogInformation("Fetching all pizza orders.");
        return await _pizzaRepo.GetAllAsync();
    }

    public async Task CancelOrderAsync(int id)
    {
        // Business Rule: You can't cancel order #1 (The Boss's Pizza)
        if (id == 1) throw new InvalidOperationException("Cannot cancel the master order!");

        await _pizzaRepo.DeleteAsync(id);
        _logger.LogWarning("Order {Id} was removed from the system.", id);
    }
}