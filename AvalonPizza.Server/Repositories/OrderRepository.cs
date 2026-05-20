using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AvalonPizza.Server.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' not found.");
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        // By opening it once at the start "Explicitly", you keep the connection open for the entire
        // "session" of that method, which is slightly more efficient for multiple result sets.
        // you ensure the connection is pulled from the pool and stays active for both the Order and the Toppings retrieval.
        // This prevents "connection toggling."
        await connection.OpenAsync();

        using var reader = await connection.QueryMultipleAsync(
            "usp_Orders_GetById", new { Id = id }, commandType: CommandType.StoredProcedure
        );

        var order = await reader.ReadSingleOrDefaultAsync<Order>();

        // safety check provided by Dapper. It ensures you don't try to read from the result stream
        // if it has already been closed or exhausted, which prevents runtime crashes.
        if (order != null && !reader.IsConsumed)
        {
            var toppings = await reader.ReadAsync<Topping>();
            order.Toppings = toppings.ToList();
        }

        return order;
    }

    public async Task<int> CreateOrderAsync(Order order)
    {
        // DataTable implements IDisposable however it can't be disposed off in the helper function
        // dispose of it to ensure that memory is freed up immediately
        using var toppingDataTable = CreateToppingDataTable(order.Toppings);

        var parameters = new DynamicParameters();
        parameters.Add("@CustomerName", order.CustomerName);
        parameters.Add("@PhoneNumber", order.PhoneNumber);
        parameters.Add("@DeliveryAddress", order.DeliveryAddress);
        parameters.Add("@SizeId", order.SizeId);
        parameters.Add("@TotalPrice", order.TotalPrice);
        parameters.Add("@Toppings", toppingDataTable.AsTableValuedParameter("dbo.ToppingListType"));

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.QuerySingleAsync<int>(
            "usp_Orders_Insert", parameters, commandType: CommandType.StoredProcedure
        );
    }

    public async Task UpdateOrderAsync(Order order)
    {
        using var toppingTable = CreateToppingDataTable(order.Toppings);

        var parameters = new DynamicParameters();
        parameters.Add("@OrderId", order.Id);
        parameters.Add("@SizeId", order.SizeId);
        parameters.Add("@TotalPrice", order.TotalPrice);
        parameters.Add("@Toppings", toppingTable.AsTableValuedParameter("dbo.ToppingListType"));

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            "usp_Orders_Update", parameters, commandType: CommandType.StoredProcedure
        );
    }

    private DataTable CreateToppingDataTable(IEnumerable<Topping> toppings)
    {
        var toppingsDataTable = new DataTable();
        toppingsDataTable.Columns.Add("ToppingId", typeof(int));

        if (toppings != null)
        {
            foreach (var topping in toppings)
            {
                toppingsDataTable.Rows.Add(topping.ToppingId);
            }
        }

        return toppingsDataTable;
    }

    public async Task UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            "usp_Orders_UpdateStatus", new { OrderId = id, StatusId = status },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task DeleteOrderAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            "usp_Orders_Delete", new { OrderId = id },
            commandType: CommandType.StoredProcedure
        );
    }
}