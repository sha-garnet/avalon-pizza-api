using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces;

public interface IToppingRepository
{
    Task<IEnumerable<Topping>> GetAllToppingsAsync();
}
