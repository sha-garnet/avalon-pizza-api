using Microsoft.Data.SqlClient;
using System.Data;

namespace AvalonPizza.Server.Repositories;

public class ToppingRepository
{

    public async Task<IEnumerable<Topping>> GetAllToppingsAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<Topping>("usp_Toppings_GetAll", commandType: CommandType.StoredProcedure);
    }
}
