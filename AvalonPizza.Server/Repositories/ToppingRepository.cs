using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AvalonPizza.Server.Repositories;

public class ToppingRepository : IToppingRepository
{
    private readonly string _connectionString;

    public ToppingRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<Topping>> GetAllToppingsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<Topping>(
            "usp_Toppings_GetAll", commandType: CommandType.StoredProcedure
        );
    }
}
