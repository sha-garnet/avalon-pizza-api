using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<int> CreateOrderAsync(Order order);
    Task UpdateOrderAsync(Order order);
    Task UpdateOrderStatusAsync(int orderId, int statusId);
    Task DeleteOrderAsync(int orderId);
}
