using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Services;

public interface IPizzaService
{
    Task<Order> ProcessOrderAsync(Order order);
    Task<IEnumerable<object>> GetAllOrdersAsync();
    Task CancelOrderAsync(int id);
    Task<Order?> UpdateOrderAsync(int id, Order updatedOrder);
}