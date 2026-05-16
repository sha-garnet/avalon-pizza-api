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
    /// Modifies the structural choices (size and toppings) of an existing pizza order.
    /// </summary>
    /// <remarks>
    /// This operation is idempotent. Modifications are strictly restricted by business rules 
    /// and will only be processed if the target order's current state is classified as 'Pending' (StatusId = 1).
    /// </remarks>
    /// <param name="id">The unique identifier of the order being targeted for modification.</param>
    /// <param name="order">The updated order payload containing modified size and topping parameters.</param>
    /// <returns>An <see cref="IActionResult"/> representing an HTTP 204 No Content response upon successful persistence.</returns>
    /// <response code="204">Returned when the order was successfully verified, recalculation was performed, and data was updated.</response>
    /// <response code="400">Returned if the input payload is null, or if the route ID does not match the body payload ID.</response>
    /// <response code="422">Returned via exception handling if the order is no longer in a mutable 'Pending' state.</response>
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