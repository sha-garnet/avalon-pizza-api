using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(int id);
    
    //Task AddAsync(Order order);
    //Task<IEnumerable<object>> GetAllAsync();
    //Task UpdateAsync(Order order);
    //Task DeleteAsync(int id);
    //Task<Order?> GetByIdAsync(int id);
    //Task<bool> CheckConnectionAsync();
}
