using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Services;

public interface IToppingService
{
    Task<IEnumerable<Topping>> GetAllToppingsAsync();
}
