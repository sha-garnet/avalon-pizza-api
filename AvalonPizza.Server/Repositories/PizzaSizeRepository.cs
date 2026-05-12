using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AvalonPizza.Server.Repositories;

public class PizzaSizeRepository : IPizzaSize
{
    private readonly string _connectionString;

    public PizzaSizeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<PizzaSize>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<PizzaSize>(
            "usp_PizzaSizes_GetAll", commandType: CommandType.StoredProcedure
        );
    }
}
