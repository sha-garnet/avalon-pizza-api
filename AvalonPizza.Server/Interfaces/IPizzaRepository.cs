using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Interfaces;

public interface IPizzaRepository
{
    void Add(PizzaOrder order);
    IEnumerable<object> GetAll();
    bool CheckConnection();
    void Update(int id, PizzaOrder order);
    void Delete(int id);
}
