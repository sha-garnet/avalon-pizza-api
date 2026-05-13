using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Repositories;

public interface IPizzaSizeRepository
{
    Task<IEnumerable<PizzaSize>> GetAllAsync();
}
