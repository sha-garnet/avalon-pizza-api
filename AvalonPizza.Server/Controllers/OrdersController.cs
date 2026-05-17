using AvalonPizza.Server.DTOs;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    /// Retrieves the complete details of a specific pizza order.
    /// </summary>
    /// <remarks>
    /// This endpoint fetches the order state, customer details,
    /// and selected pizza respective size and toppings.
    /// </remarks>
    /// <param name="id">The unique identifier of the target order.</param>
    /// <response code="200">Returned successfully with the matching order payload.</response>
    /// <response code="404">Returned if no order matching the specified ID exists.</response>
    /// <response code="500">Returned if an unhandled server error occurs.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Submits a new pizza order.
    /// </summary>
    /// <remarks>
    /// This endpoint initializes a new order record with a default 'Pending' lifecycle state.
    /// </remarks>
    /// <param name="order">The payload containing customer details and selected pizza line items.</param>
    /// <response code="201">Order created successfully. Returns the finalized order identifier.</response>
    /// <response code="400">Returned if the request payload fails validation rules (e.g., missing mandatory customer fields or invalid pizza configurations).</response>
    /// <response code="500">Returned if an unhandled server error occurs.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> CreateOrder([FromBody] OrderRequest order)
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
    /// Modifications will only be processed if the target order's current state is classified as 'Pending' (StatusId = 1).
    /// </remarks>
    /// <param name="id">The unique identifier of the order being targeted for modification.</param>
    /// <param name="order">The updated order payload containing modified size and topping parameters.</param>
    /// <response code="204">Returned when the order was successfully verified, recalculation was performed, and data was updated.</response>
    /// <response code="400">Returned if the request payload fails validation rules.</response>
    /// <response code="422">Returned if the order is no longer in a mutable 'Pending' state.</response>
    /// <response code="500">Returned if an unhandled server error occurs.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderRequest order)
    {
        if (order == null)
        {
            return BadRequest("Order payload cannot be empty.");
        }

        await _orderService.UpdateOrderAsync(id, order);
        return NoContent();
    }

    /// <summary>
    /// Modifies the processing state of an existing order.
    /// </summary>
    /// <remarks>
    /// This is an administrative operation locked behind authorization filters. It advances
    /// the order through its manufacturing lifecycle (e.g., 1 [Pending] → 2 [Baking] → 3 [Out for Delivery]).
    /// </remarks>
    /// <param name="id">The unique identifier of the targeted order.</param>
    /// <param name="statusId">The target status lookup identifier to apply to the record.</param>
    /// <response code="204">Status updated successfully.</response>
    /// <response code="400">Returned if the target statusId is invalid, or if trying to perform an illegal state transition (e.g., moving an order from 'Cancelled' back to 'Baking').</response>
    /// <response code="401">Returned if the request lacks a valid identity token or bearer authentication credentials.</response>
    /// <response code="404">Returned if no order matching the specified order ID can be located.</response>
    /// <response code="500">Returned if an unhandled server error occurs.</response>
    [Authorize]
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] int statusId)
    {
        if (statusId <= 0)
        {
            return BadRequest("Invalid Status ID parameter specified.");
        }

        await _orderService.UpdateOrderStatusAsync(id, statusId);
        return NoContent();
    }

    /// <summary>
    /// Performs a safe soft-delete on a targeted customer order.
    /// </summary>
    /// <remarks>
    /// Deletion is strictly restricted by business rules. The operation will only succeed
    /// if the target order exists and its current state is 'Pending' (StatusId = 1).
    /// Records are soft-deleted by flipping their active status flag, maintaining historical audit integrity.
    /// </remarks>
    /// <param name="id">The unique identifier of the targeted order.</param>
    /// <response code="204">Order successfully processed and marked as soft-deleted.</response>
    /// <response code="400">Returned if the incoming order ID format is invalid.</response>
    /// <response code="404">Returned if no order matching the specified ID exists within.</response>
    /// <response code="422">Returned if the order is in a state that cannot be modified (e.g., already 'Baking', 'Out for Delivery', or 'Completed').</response>
    /// <response code="500">Returned if an unhandled server error occurs.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        // Simple defensive boundary check
        if (id <= 0)
        {
            return BadRequest("Invalid order identifier specified.");
        }

        await _orderService.DeleteOrderAsync(id);
        return NoContent();
    }
}