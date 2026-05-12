using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Repositories;

public interface IToppingRepository
{
    Task<IEnumerable<Topping>> GetAllToppingsAsync();
}
