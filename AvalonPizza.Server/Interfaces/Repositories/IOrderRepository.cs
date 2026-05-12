using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(int id);
    Task<int> CreateOrderAsync(Order order);
}
