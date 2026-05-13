using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Services;

public interface IOrderService
{
    Task<Order?> GetOrderDetailsAsync(int id);
    Task<int> PlaceOrderAsync(Order order);
}