using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace AvalonPizza.Server.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: api/orders
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger)
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

    /// <summary>
    /// 
    /// [FromBody]: This tells .NET to look at the request body for JSON and map it directly into your Order model
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
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