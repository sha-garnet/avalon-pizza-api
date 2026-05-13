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

    public async Task<IEnumerable<PizzaSize>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<PizzaSize>(
            "usp_PizzaSizes_GetAll", commandType: CommandType.StoredProcedure
        );
    }
}
