using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Models;
using AvalonPizza.Server.Options;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AvalonPizza.Server.Repositories;

public class ToppingRepository : IToppingRepository
{
    private readonly string _connectionString;

    public ToppingRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<IEnumerable<Topping>> GetAllToppingsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QueryAsync<Topping>(
            "usp_Toppings_GetAll", commandType: CommandType.StoredProcedure
        );
    }
}
