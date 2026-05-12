using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AvalonPizza.Server.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        using var multi = await connection.QueryMultipleAsync(
            "usp_Orders_GetById", new { Id = id }, commandType: CommandType.StoredProcedure
        );

        var order = await multi.ReadSingleOrDefaultAsync<Order>();

        if (order != null)
        {
            var toppings = await multi.ReadAsync<Topping>();
            order.Toppings = toppings.ToList();
        }

        return order;
    }

    //// [CREATE]
    //public async Task AddAsync(Order order)
    //{
    //    using var conn = new SqlConnection(_connectionString);
    //    var parameters = new
    //    {
    //        order.Size,
    //        Toppings = string.Join(", ", order.Toppings),
    //        order.Price
    //    };
    //    await conn.ExecuteAsync("AddPizza", parameters, commandType: CommandType.StoredProcedure);
    //}

    //// [READ]
    //public async Task<IEnumerable<object>> GetAllAsync()
    //{
    //    using var conn = new SqlConnection(_connectionString);
    //    return await conn.QueryAsync("GetPizzas", commandType: CommandType.StoredProcedure);
    //}

    //// [UPDATE]
    //public async Task UpdateAsync(Order order)
    //{
    //    using var conn = new SqlConnection(_connectionString);
    //    var parameters = new
    //    {
    //        order.Id,
    //        order.Size,
    //        Toppings = string.Join(", ", order.Toppings),
    //        order.Price
    //    };
    //    await conn.ExecuteAsync("UpdatePizza", parameters, commandType: CommandType.StoredProcedure);
    //}

    //// [DELETE]
    //public async Task DeleteAsync(int id)
    //{
    //    using var conn = new SqlConnection(_connectionString);
    //    await conn.ExecuteAsync("DeletePizza", new
    //    {
    //        Id = id
    //    }, commandType: CommandType.StoredProcedure);
    //}

    //public async Task<bool> CheckConnectionAsync()
    //{
    //    try
    //    {
    //        using var conn = new SqlConnection(_connectionString);
    //        return await conn.ExecuteScalarAsync<int>("SELECT 1") == 1;
    //    }
    //    catch
    //    {
    //        return false;
    //    }
    //}
}