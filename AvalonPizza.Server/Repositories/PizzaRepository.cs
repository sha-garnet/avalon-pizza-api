using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AvalonPizza.Server.Repositories;

public class PizzaRepository : IPizzaRepository
{
    private readonly string _connectionString;

    public PizzaRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool CheckConnection()
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            return true;
        }
        catch { return false; }
    }

    public void Add(PizzaOrder order)
    {
        string toppingsCsv = string.Join(", ", order.Toppings);
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("AddPizza", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Size", order.Size);
        cmd.Parameters.AddWithValue("@Toppings", toppingsCsv);
        cmd.Parameters.AddWithValue("@Price", order.Price);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public IEnumerable<object> GetAll()
    {
        var orders = new List<object>();
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("GetPizzas", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            orders.Add(new
            {
                Id = reader["Id"],
                Size = reader["Size"],
                Toppings = reader["Toppings"].ToString()?.Split(", "),
                Price = reader["Price"]
            });
        }
        return orders;
    }

    public void Update(int id, PizzaOrder order)
    {
        string toppingsCsv = string.Join(", ", order.Toppings);
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("UpdatePizza", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Size", order.Size);
        cmd.Parameters.AddWithValue("@Toppings", toppingsCsv);
        cmd.Parameters.AddWithValue("@Price", order.Price);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("DeletePizza", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Id", id);

        conn.Open();
        cmd.ExecuteNonQuery();
    }
}