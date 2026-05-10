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

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        bool isAlive = _pizzaRepo.CheckConnection();
        return isAlive ? Ok("Healthy") : StatusCode(503, new { status = "Unhealthy", database = "Database Offline" });
    }

    // POST: Add a new order to the list
    [HttpPost]
    public IActionResult PlaceOrder([FromBody] PizzaOrder order)
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
            _pizzaRepo.Add(order);
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
    public IActionResult GetAllOrders()
    {
        return Ok(_pizzaRepo.GetAll());
    }

    // [UPDATE] - PUT: api/pizza/1
    [HttpPut("{id}")]
    public IActionResult UpdateOrder(int id, [FromBody] PizzaOrder order)
    {
        // Re-calculate the price based on updated info
        decimal basePrice = order.Size == "Small" ? 8 : (order.Size == "Medium" ? 10 : 12);
        order.Price = basePrice + (order.Toppings.Count * 1.50m);

        _pizzaRepo.Update(id, order);
        return Ok(new { message = $"Order {id} updated successfully!" });
    }

    // [DELETE] - DELETE: api/pizza/1
    [HttpDelete("{id}")]
    public IActionResult CancelOrder(int id)
    {
        _pizzaRepo.Delete(id);
        return Ok(new { message = $"Order {id} deleted." });
    }
}