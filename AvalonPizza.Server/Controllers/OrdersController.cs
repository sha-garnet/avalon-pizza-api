using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AvalonPizza.Server.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
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
    /// Fetches a specific order and its associated toppings.
    /// GET: /api/orders/{id}
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Order</returns>
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
    /// Places a new order.
    /// POST: /api/orders
    /// [FromBody]: tells .NET to look at the request body for JSON and map it directly into your Order model
    /// </summary>
    /// <param name="order"></param>
    /// <returns>The newly created Order ID.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(int))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> CreateOrder([FromBody] Order order)
    {
        if (order == null)
        {
            return BadRequest("Order payload cannot be empty.");
        }

        int newOrderId = await _orderService.PlaceOrderAsync(order);
        // Returns HTTP 201 with Location header pointing to your GET endpoint
        return CreatedAtAction(nameof(GetOrder), new { id = newOrderId }, newOrderId);
    }

    /// <summary>
    /// Updates order pizza size and pizza toppings.
    /// Only allowed if status is Pending in data store.
    /// PUT: api/orders/{id}
    /// </summary>
    /// <param name="id"></param>
    /// <param name="orderDto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
    {
        if (order == null)
        {
            return BadRequest("Order payload cannot be empty.");
        }

        if (id != order.Id)
        {
            return BadRequest("ID mismatch between URL and body.");
        }

        await _orderService.UpdateOrderAsync(order);
        return NoContent();
    }

    /// <summary>
    /// Admin only operation
    /// Updates the order lifecycle (e.g., Pending → Baking)
    /// PATCH: api/orders/{id}/status
    /// </summary>
    /// <param name="id"></param>
    /// <param name="statusId"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] int statusId)
    {
        await _orderService.UpdateOrderStatusAsync(id, statusId);
        return NoContent();
    }

    /// <summary>
    /// Performs a soft-delete. Only allowed if status is Pending.
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