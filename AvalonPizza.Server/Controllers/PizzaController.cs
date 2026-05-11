using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Models;
using AvalonPizza.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Serilog.Core;

namespace AvalonPizza.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzaController : ControllerBase
{
    private readonly IPizzaService _pizzaService;
    private readonly IPizzaRepository _pizzaRepo;
    private readonly ILogger<PizzaController> _logger;

    public PizzaController(
        IPizzaService pizzaService,
        IPizzaRepository pizzaRepo,
        ILogger<PizzaController> logger)
    {
        _pizzaService = pizzaService;
        _pizzaRepo = pizzaRepo;
        _logger = logger;
    }

    // POST: Add a new order to the list
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PizzaOrder order)
    {

        _logger.LogInformation("Received a new order request for a {Size} pizza.", order.Size);

        var processedOrder = await _pizzaService.ProcessOrderAsync(order);

        return Ok(new
        {
            message = "Order Placed!",
            totalPrice = processedOrder.Price.ToString("C"), // Formats as currency like $13.50
            orderDetails = processedOrder
        });
    }

    // GET: Retrieve all orders
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _pizzaService.GetAllOrdersAsync();
        return Ok(orders);
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
        await _pizzaService.CancelOrderAsync(id);
        return Ok(new { message = $"Order {id} has been cancelled." });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        bool isAlive = await _pizzaRepo.CheckConnectionAsync();
        return isAlive ? Ok("Healthy") : StatusCode(503, new { status = "Unhealthy", database = "Database Offline" });
    }
}