using Microsoft.AspNetCore.Mvc;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzaController : ControllerBase
{
    private static int _nextId = 1; // Counter for unique IDs
    // This 'static' list stays in memory as long as the app is running
    private static List<PizzaOrder> _orders = new List<PizzaOrder>();

    // 1. POST: Add a new order to the list
    [HttpPost]
    public IActionResult PlaceOrder([FromBody] PizzaOrder order)
    {
        order.Id = _nextId++; // Assign unique ID

        // 1. Calculate Base Price based on Size
        decimal basePrice = order.Size switch
        {
            "Small" => 8.00m,
            "Medium" => 10.00m,
            "Large" => 12.00m,
            _ => 0m
        };

        // 2. Calculate Topping Price ($1.50 per topping)
        decimal toppingPrice = order.Toppings.Count * 1.50m;

        // 3. Set the total price
        order.Price = basePrice + toppingPrice;

        // 4. Save to our in-memory list
        _orders.Add(order);

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
        return Ok(_orders); // ASP.NET Core automatically converts the List to JSON
    }

    // [UPDATE] - PUT: api/pizza/1
    [HttpPut("{id}")]
    public IActionResult UpdateOrder(int id, [FromBody] PizzaOrder updatedOrder)
    {
        var existingOrder = _orders.FirstOrDefault(o => o.Id == id);
        if (existingOrder == null) return NotFound("Order not found.");

        // Update details
        existingOrder.Size = updatedOrder.Size;
        existingOrder.Toppings = updatedOrder.Toppings;

        // Recalculate price
        decimal basePrice = existingOrder.Size == "Small" ? 8 : (existingOrder.Size == "Medium" ? 10 : 12);
        existingOrder.Price = basePrice + (existingOrder.Toppings.Count * 1.50m);

        return Ok(new { message = "Order updated!", order = existingOrder });
    }

    // [DELETE] - DELETE: api/pizza/1
    [HttpDelete("{id}")]
    public IActionResult CancelOrder(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null) return NotFound();

        _orders.Remove(order);
        return Ok($"Order {id} has been canceled.");
    }
}