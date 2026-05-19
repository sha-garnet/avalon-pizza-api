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
    /// Retrieves a complete list of all active pizza toppings and their corresponding prices.
    /// </summary>
    /// <remarks>
    /// This query is optimized via a Redis distributed cache layer to minimize database round-trips.
    /// </remarks>
    /// <response code="200">Returned successfully with the array of active topping configurations and pricing metrics.</response>
    /// <response code="404">Returned if the topping catalog is empty in the storage tier.</response>
    /// <response code="500">Returned if an unhandled server error occurs.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Topping>>> GetAll()
    {
        var toppings = await _toppingService.GetAllToppingsAsync();
        if (toppings == null || !toppings.Any())
        {
            _logger.LogError("Pizza toppings requested but none were found in the database.");
            return NotFound("Pizza toppings are currently unavailable. Please check back later.");
        }
        return Ok(toppings);
    }
}