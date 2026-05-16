using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IToppingService _toppingService;
    private readonly IPizzaSizeService _pizzaSizeService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IToppingService toppingService,
        IPizzaSizeService pizzaSizeService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _toppingService = toppingService;
        _pizzaSizeService = pizzaSizeService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the fully populated details of a specific order by its unique identifier.
    /// </summary>
    /// <remarks>
    /// Acts as the core orchestration point for order data retrieval, passing the request 
    /// down to the data access layer. Returns null if no matching active record is located.
    /// </remarks>
    /// <param name="id">The unique database identifier of the order to retrieve.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the populated <see cref="Order"/> 
    /// graph if found; otherwise, <see langword="null"/>.
    /// </returns>
    public async Task<Order?> GetOrderDetailsAsync(int id)
    {
        return await _orderRepository.GetOrderByIdAsync(id);
    }

    /// <summary>
    /// Orchestrates the placement of a new customer pizza order.
    /// Validates pricing on the server side before persisting the record to the database.
    /// </summary>
    /// <param name="order">The incoming order payload to process.</param>
    /// <returns>The newly generated unique Order ID.</returns>
    public async Task<int> PlaceOrderAsync(Order order)
    {
        order.TotalPrice = await CalculatePriceAsync(order);
        return await _orderRepository.CreateOrderAsync(order);
    }

    /// <summary>
    /// Orchestrates the modification of an existing order's item components.
    /// Integrity-checks and recalculates server-verified pricing before committing changes to the database.
    /// </summary>
    /// <param name="order">The updated domain order model containing the targeted modifications.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the target order identifier cannot be found, or if selected component lookups fail validation.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the database validation rejects the execution because the order is no longer in a mutable 'Pending' state.
    /// </exception>
    public async Task UpdateOrderAsync(Order order)
    {
        order.TotalPrice = await CalculatePriceAsync(order);
        await _orderRepository.UpdateOrderAsync(order);
    }

    /// <summary>
    /// Calculates the total verified server-side price for a given order payload.
    /// Utilizes Redis cached lookup services to cross-reference sizes and toppings,
    /// ensuring price integrity and guarding against client-side price tampering.
    /// </summary>
    /// <param name="order">The incoming order object containing the selected SizeId and Toppings collection.</param>
    /// <returns>The total compounded price (<see cref="decimal"/>) including the base size price and all selected toppings.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the provided <paramref name="order.SizeId"/> or any structural <c>ToppingId</c> 
    /// cannot be verified against the data store lookup records.
    /// </exception>
    private async Task<decimal> CalculatePriceAsync(Order order)
    {
        var availableSizes = await _pizzaSizeService.GetAllSizesAsync();
        var availableToppings = await _toppingService.GetAllToppingsAsync();

        var selectedSize = availableSizes.FirstOrDefault(s => s.SizeId == order.SizeId)
            ?? throw new KeyNotFoundException($"Invalid Pizza Size ID '{order.SizeId}' selected.");

        decimal totalPrice = selectedSize.BasePrice;

        foreach (var orderTopping in order.Toppings)
        {
            var toppingInfo = availableToppings.FirstOrDefault(t => t.ToppingId == orderTopping.ToppingId)
                ?? throw new KeyNotFoundException($"Invalid Topping ID '{orderTopping.ToppingId}' selected.");
            totalPrice += toppingInfo.ToppingPrice;
        }

        return totalPrice;
    }

    public async Task UpdateOrderStatusAsync(int orderId, int statusId)
    {
        await _orderRepository.UpdateOrderStatusAsync(orderId, statusId);
    }

    public async Task DeleteOrderAsync(int orderId)
    {
        await _orderRepository.DeleteOrderAsync(orderId);
    }
}