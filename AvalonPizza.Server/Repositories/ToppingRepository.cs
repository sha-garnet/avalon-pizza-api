using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AvalonPizza.Server.Repositories;

public class ToppingRepository : IToppingRepository
{
    private readonly string _connectionString;

    public ToppingRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' not found.");
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
