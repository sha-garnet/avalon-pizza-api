using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace AvalonPizza.Server.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: api/orders
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _orderService.GetOrderDetailsAsync(id);

        if (order == null)
        {
            return NotFound(new { Message = $"Order #{id} not found." });
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateOrder([FromBody] Order order)
    {
        try
        {
            // Basic validation
            if (order == null || order.SizeId <= 0)
            {
                return BadRequest("Invalid order data provided.");
            }

            // Call the service to calculate price and save to DB
            int newOrderId = await _orderService.PlaceOrderAsync(order);

            // Returns a 201 Created status with the link to the new resource
            return CreatedAtAction(nameof(GetOrder), new { id = newOrderId }, newOrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating an order for {CustomerName}", order?.CustomerName);
            return StatusCode(500, "An internal error occurred while processing your order.");
        }
    }
}