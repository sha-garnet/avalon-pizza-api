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
    public async Task<ActionResult<int>> CreateOrder([FromBody] Order order) // [FromBody]: This tells .NET to look at the request body for JSON and map it directly into your Order model
    {
        try
        {
            // Basic validation
            if (order == null || order.SizeId <= 0)
            {
                return BadRequest("Invalid order data provided.");
            }

            int newOrderId = await _orderService.PlaceOrderAsync(order);

            // Returns a 201 Created status with the link to the new resource
            // (Generates URL) It looks at your GetOrder method's route (api/orders/{id}) and fills in the {id} with your newOrderId
            // It adds a Location field to the response metadata => Location: https://api.avalonpizza.com/api/orders/1024
            return CreatedAtAction(nameof(GetOrder), new { id = newOrderId }, newOrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating an order for {CustomerName}", order?.CustomerName);
            return StatusCode(500, "An internal error occurred while processing your order.");
        }
    }
}