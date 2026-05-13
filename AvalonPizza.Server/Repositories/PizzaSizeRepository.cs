using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AvalonPizza.Server.Repositories;

public class PizzaSizeRepository : IPizzaSizeRepository
{
    private readonly string _connectionString;

    public PizzaSizeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' not found.");
    }

    // return type is read-only and relatively static so IEnumerable return type is perfect
    public async Task<IEnumerable<PizzaSize>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<PizzaSize>(
            "usp_PizzaSizes_GetAll", commandType: CommandType.StoredProcedure
        );
    }
}
