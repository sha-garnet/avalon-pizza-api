using AvalonPizza.Server.DTOs;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Services;

public interface IOrderService
{
    Task<Order?> GetOrderDetailsAsync(int id);
    Task<int> PlaceOrderAsync(OrderRequest order);
    Task UpdateOrderAsync(int id, OrderRequest order);
    Task UpdateOrderStatusAsync(int id, StatusRequest statusRequest);
    Task DeleteOrderAsync(int Id);
}