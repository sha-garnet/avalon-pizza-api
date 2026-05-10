using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces;

public interface IPizzaRepository
{
    Task AddAsync(PizzaOrder order);
    Task<IEnumerable<object>> GetAllAsync();
    Task UpdateAsync(int id, PizzaOrder order);
    Task DeleteAsync(int id);
    Task<bool> CheckConnectionAsync();
}
