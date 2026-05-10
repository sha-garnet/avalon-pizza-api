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
    public async Task AddAsync(PizzaOrder order)
    {
        using var conn = new SqlConnection(_connectionString);
        // Dapper maps the 'order' object properties to @Size, @Toppings, etc. automatically!
        var parameters = new
        {
            Size = order.Size,
            Toppings = string.Join(", ", order.Toppings),
            Price = order.Price
        };

        await conn.ExecuteAsync("AddPizza", parameters, commandType: CommandType.StoredProcedure);
    }

    // [READ]
    public async Task<IEnumerable<object>> GetAllAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        // Dapper runs the procedure and returns a collection of objects
        return await conn.QueryAsync("GetPizzas", commandType: CommandType.StoredProcedure);
    }

    // [UPDATE]
    public async Task UpdateAsync(int id, PizzaOrder order)
    {
        using var conn = new SqlConnection(_connectionString);
        var parameters = new
        {
            Id = id,
            Size = order.Size,
            Toppings = string.Join(", ", order.Toppings),
            Price = order.Price
        };
        await conn.ExecuteAsync("UpdatePizza", parameters, commandType: CommandType.StoredProcedure);
    }

    // [DELETE]
    public async Task DeleteAsync(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.ExecuteAsync("DeletePizza", new { Id = id }, commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> CheckConnectionAsync()
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<int>("SELECT 1") == 1;
        }
        catch { return false; }
    }
}