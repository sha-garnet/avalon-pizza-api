using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Services;

public interface IPizzaService
{
    Task<PizzaOrder> ProcessOrderAsync(PizzaOrder order);
    Task<IEnumerable<object>> GetAllOrdersAsync();
    Task CancelOrderAsync(int id);

}