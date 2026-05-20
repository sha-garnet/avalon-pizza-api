using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(int id);
    Task<int> CreateOrderAsync(Order order);
    Task UpdateOrderAsync(Order order);
    Task UpdateOrderStatusAsync(int id, OrderStatus status);
    Task DeleteOrderAsync(int id);
}
