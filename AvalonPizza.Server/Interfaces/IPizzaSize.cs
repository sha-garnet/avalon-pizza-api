using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces;

public interface IPizzaSize
{
    Task<IEnumerable<PizzaSize>> GetAllAsync();
}
