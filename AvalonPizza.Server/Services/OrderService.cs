using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IToppingRepository _toppingRepository;
    private readonly IPizzaSizeRepository _pizzaSizeRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IToppingRepository toppingRepository,
        IPizzaSizeRepository pizzaSizeRepository,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _toppingRepository = toppingRepository;
        _pizzaSizeRepository = pizzaSizeRepository;
        _logger = logger;
    }

    public async Task<Order?> GetOrderDetailsAsync(int id)
    {
        return await _orderRepository.GetOrderByIdAsync(id);
    }

    public async Task<int> PlaceOrderAsync(Order order)
    {
        var availableSizes = await _pizzaSizeRepository.GetAllAsync();
        var availableToppings = await _toppingRepository.GetAllToppingsAsync();

        var selectedSize = availableSizes.FirstOrDefault(s => s.SizeId == order.SizeId);
        if (selectedSize == null)
            throw new Exception("Invalid Pizza Size selected.");

        decimal totalPrice = selectedSize.BasePrice;

        foreach (var orderTopping in order.Toppings)
        {
            var toppingInfo = availableToppings.FirstOrDefault(t => t.ToppingId == orderTopping.ToppingId);
            if (toppingInfo != null)
            {
                totalPrice += toppingInfo.ToppingPrice;
            }
        }

        order.TotalPrice = totalPrice;

        return await _orderRepository.CreateOrderAsync(order);
    }

    public async Task UpdateOrderAsync(Order order)
    {
        var availableSizes = await _pizzaSizeRepository.GetAllAsync();
        var availableToppings = await _toppingRepository.GetAllToppingsAsync();

        var selectedSize = availableSizes.FirstOrDefault(s => s.SizeId == order.SizeId);
        if (selectedSize == null)
            throw new Exception("Invalid Pizza Size selected.");

        decimal totalPrice = selectedSize.BasePrice;

        foreach (var orderTopping in order.Toppings)
        {
            var toppingInfo = availableToppings.FirstOrDefault(t => t.ToppingId == orderTopping.ToppingId);
            if (toppingInfo != null)
            {
                totalPrice += toppingInfo.ToppingPrice;
            }
        }

        order.TotalPrice = totalPrice;

        await _orderRepository.UpdateOrderAsync(order);
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