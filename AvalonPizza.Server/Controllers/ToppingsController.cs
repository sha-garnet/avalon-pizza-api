using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace AvalonPizza.Server.Controllers;

[ApiController]
[Route("api/toppings")]
[Produces("application/json")]
public class ToppingsController : ControllerBase
{
    private readonly IToppingService _toppingService;
    private readonly ILogger<ToppingsController> _logger;

    public ToppingsController(IToppingService toppingService, ILogger<ToppingsController> logger)
    {
        _toppingService = toppingService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a complete list of all active pizza toppings.
    /// </summary>
    /// <remarks>
    /// This query is heavily optimized via a Redis distributed cache layer to minimize database round-trips. 
    /// Use this endpoint to populate dynamic UI components, such as pizza configuration screens or ordering menus.
    /// </remarks>
    /// <returns>A collection containing all active <see cref="Topping"/> entities.</returns>
    /// <response code="200">Returned successfully with the array of active topping configurations.</response>
    /// <response code="404">Returned if the topping catalog is empty or temporarily unpopulated in the storage tier.</response>
    /// <response code="500">Returned if an unhandled error occurs while communicating with the cache or database cluster.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Topping>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Topping>>> GetAll()
    {
        var toppings = await _toppingService.GetAllToppingsAsync();
        if (toppings == null || !toppings.Any())
        {
            _logger.LogError("Toppings requested but none were found in the database.");
            return NotFound("Toppings are currently unavailable. Please check back later.");
        }
        return Ok(toppings);
    }
}