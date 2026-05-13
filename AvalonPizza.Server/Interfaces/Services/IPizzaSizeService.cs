using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Services;

public interface IPizzaSizeService
{
    Task<IEnumerable<PizzaSize>> GetAllSizesAsync();
}
