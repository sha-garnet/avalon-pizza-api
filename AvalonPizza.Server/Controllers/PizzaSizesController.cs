using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace AvalonPizza.Server.Controllers;

[ApiController]
[Route("api/pizzas/sizes")]
[Produces("application/json")]
public class PizzaSizesController : ControllerBase
{
    private readonly IPizzaSizeService _pizzaSizeService;
    private readonly ILogger<PizzaSizesController> _logger;

    public PizzaSizesController(
        IPizzaSizeService pizzaSizeService,
        ILogger<PizzaSizesController> logger)
    {
        _pizzaSizeService = pizzaSizeService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a complete list of all available pizza sizes and their corresponding base prices.
    /// </summary>
    /// <remarks>
    /// This query is optimized via a Redis distributed cache layer to minimize database round-trips.
    /// </remarks>
    /// <response code="200">Returned successfully with the array of active configured pizza sizes and pricing metrics.</response>
    /// <response code="404">Returned if the pizza sizes catalog is empty in the storage tier.</response>
    /// <response code="500">Returned if an unhandled server error occurs.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PizzaSize>>> GetAll()
    {
        var sizes = await _pizzaSizeService.GetAllSizesAsync();
        if (sizes == null || !sizes.Any())
        {
            _logger.LogError("Pizza sizes requested but none were found in the database.");
            return NotFound("Pizza sizes are currently unavailable. Please check back later.");
        }
        return Ok(sizes);
    }
}