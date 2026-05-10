using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AvalonPizza.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzaController : ControllerBase
{
    private readonly IPizzaRepository _pizzaRepo;

    public PizzaController(IPizzaRepository pizzaRepo)
    {
        _pizzaRepo = pizzaRepo;
    }

    // POST: Add a new order to the list
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PizzaOrder order)
    {
        decimal basePrice = order.Size switch
        {
            "Small" => 8.00m,
            "Medium" => 10.00m,
            "Large" => 12.00m,
            _ => 0m
        };
        decimal toppingPrice = order.Toppings.Count * 1.50m;
        order.Price = basePrice + toppingPrice;


        try
        {
            await _pizzaRepo.AddAsync(order);
        }
        catch (SqlException ex)
        {
            // Log the error here (ex.Message)
            return StatusCode(503, new
            {
                message = "Database is currently unavailable. Please try again later.",
                code = ex.Number // SQL Error numbers are helpful for debugging
            });
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Something went wrong with your order." });
        }


        return Ok(new
        {
            message = "Order Placed!",
            totalPrice = order.Price.ToString("C"), // Formats as currency like $13.50
            orderDetails = order
        });
    }

    // GET: Retrieve all orders
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        return Ok(await _pizzaRepo.GetAllAsync());
    }

    // [UPDATE] - PUT: api/pizza/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] PizzaOrder order)
    {
        // Re-calculate the price based on updated info
        decimal basePrice = order.Size == "Small" ? 8 : (order.Size == "Medium" ? 10 : 12);
        order.Price = basePrice + (order.Toppings.Count * 1.50m);

        await _pizzaRepo.UpdateAsync(id, order);
        return Ok(new { message = $"Order {id} updated successfully!" });
    }

    // [DELETE] - DELETE: api/pizza/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        await _pizzaRepo.DeleteAsync(id);
        return Ok(new { message = $"Order {id} deleted." });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        bool isAlive = await _pizzaRepo.CheckConnectionAsync();
        return isAlive ? Ok("Healthy") : StatusCode(503, new { status = "Unhealthy", database = "Database Offline" });
    }
}