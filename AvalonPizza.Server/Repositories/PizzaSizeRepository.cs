using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Models;
using AvalonPizza.Server.Options;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AvalonPizza.Server.Repositories;

public class PizzaSizeRepository : IPizzaSizeRepository
{
    private readonly string _connectionString;

    public PizzaSizeRepository(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
    }

    public async Task<IEnumerable<PizzaSize>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return await connection.QueryAsync<PizzaSize>(
            "usp_PizzaSizes_GetAll", commandType: CommandType.StoredProcedure
        );
    }
}
