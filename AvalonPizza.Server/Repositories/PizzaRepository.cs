using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace AvalonPizza.Server.Repositories;

public class PizzaRepository : IPizzaRepository
{
    private readonly string _connectionString;

    public PizzaRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // [CREATE]
    public void Add(PizzaOrder order)
    {
        using var conn = new SqlConnection(_connectionString);
        // Dapper maps the 'order' object properties to @Size, @Toppings, etc. automatically!
        var parameters = new
        {
            Size = order.Size,
            Toppings = string.Join(", ", order.Toppings),
            Price = order.Price
        };

        conn.Execute("AddPizza", parameters, commandType: CommandType.StoredProcedure);
    }

    // [READ]
    public IEnumerable<object> GetAll()
    {
        using var conn = new SqlConnection(_connectionString);
        // Dapper runs the procedure and returns a collection of objects
        return conn.Query("GetPizzas", commandType: CommandType.StoredProcedure);
    }

    // [UPDATE]
    public void Update(int id, PizzaOrder order)
    {
        using var conn = new SqlConnection(_connectionString);
        var parameters = new
        {
            Id = id,
            Size = order.Size,
            Toppings = string.Join(", ", order.Toppings),
            Price = order.Price
        };

        conn.Execute("UpdatePizza", parameters, commandType: CommandType.StoredProcedure);
    }

    // [DELETE]
    public void Delete(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Execute("DeletePizza", new { Id = id }, commandType: CommandType.StoredProcedure);
    }

    public bool CheckConnection()
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            return conn.ExecuteScalar<int>("SELECT 1") == 1;
        }
        catch { return false; }
    }
}