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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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

    /// <summary>
    /// PUT: api/orders/{id}
    /// </summary>
    /// <param name="id"></param>
    /// <param name="orderDto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
    {
        if (id != order.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        // The Service calls the Repo, which calls the Stored Proc.
        // Our Middleware will catch any SQL Exceptions (like "Order not Pending").
        await _orderService.UpdateOrderAsync(order);

        return NoContent();
    }

    /// <summary>
    /// PATCH: api/orders/{id}/status
    /// </summary>
    /// <param name="id"></param>
    /// <param name="statusId"></param>
    /// <returns></returns>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] int statusId)
    {
        await _orderService.UpdateOrderStatusAsync(id, statusId);
        return NoContent();
    }

    /// <summary>
    /// DELETE: api/orders/{id}
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        await _orderService.DeleteOrderAsync(id);
        return NoContent();
    }
}