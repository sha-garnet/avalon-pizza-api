using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Repositories;

public interface IPizzaSize
{
    Task<IEnumerable<PizzaSize>> GetAllAsync();
}
