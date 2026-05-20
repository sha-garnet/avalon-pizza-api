using AutoMapper;
using AvalonPizza.Server.DTOs;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IToppingService _toppingService;
    private readonly IPizzaSizeService _pizzaSizeService;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IToppingService toppingService,
        IPizzaSizeService pizzaSizeService,
        IMapper mapper,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _toppingService = toppingService;
        _pizzaSizeService = pizzaSizeService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Order?> GetOrderDetailsAsync(int id)
    {
        return await _orderRepository.GetOrderByIdAsync(id);
    }

    public async Task<int> PlaceOrderAsync(OrderRequest orderRequest)
    {
        // convert OrderRequest DTO into the database Order entity
        var order = _mapper.Map<Order>(orderRequest);
        order.TotalPrice = await CalculatePriceAsync(order);
        return await _orderRepository.CreateOrderAsync(order);
    }

    public async Task UpdateOrderAsync(int id, UpdateRequest updateRequest)
    {
        var order = _mapper.Map<Order>(updateRequest);
        order.Id = id;
        order.TotalPrice = await CalculatePriceAsync(order);
        await _orderRepository.UpdateOrderAsync(order);
    }

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

    public async Task UpdateOrderStatusAsync(int id, StatusRequest statusRequest)
    {
        await _orderRepository.UpdateOrderStatusAsync(id, statusRequest.Status);
    }

    public async Task DeleteOrderAsync(int id)
    {
        await _orderRepository.DeleteOrderAsync(id);
    }
}