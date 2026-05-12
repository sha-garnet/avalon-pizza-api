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

    public async Task<int> CreateOrderAsync(Order order)
    {
        var toppingDataTable = new DataTable();
        toppingDataTable.Columns.Add("ToppingId", typeof(int));

        foreach (var topping in order.Toppings)
        {
            toppingDataTable.Rows.Add(topping.ToppingId);
        }

        var parameters = new DynamicParameters();
        parameters.Add("@CustomerName", order.CustomerName);
        parameters.Add("@PhoneNumber", order.PhoneNumber);
        parameters.Add("@DeliveryAddress", order.DeliveryAddress);
        parameters.Add("@SizeId", order.SizeId);
        parameters.Add("@TotalPrice", order.TotalPrice);
        parameters.Add("@Toppings", toppingDataTable.AsTableValuedParameter("dbo.ToppingListType"));

        using var connection = new SqlConnection(_connectionString);

        return await connection.QuerySingleAsync<int>(
            "usp_Orders_Insert", parameters, commandType: CommandType.StoredProcedure
        );
    }
}