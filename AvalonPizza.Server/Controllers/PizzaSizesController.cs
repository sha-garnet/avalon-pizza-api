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
    /// This lookup endpoint is backed by a Redis distributed cache layer to guarantee sub-millisecond response times. 
    /// Use this resource to build size-selection UI controls and initiate base pricing calculations on the client application.
    /// </remarks>
    /// <returns>A collection containing all active <see cref="PizzaSize"/> options.</returns>
    /// <response code="200">Returned successfully with the array of configured pizza sizes and pricing metrics.</response>
    /// <response code="404">Returned if the catalog lookup tables are completely unpopulated in the storage tier.</response>
    /// <response code="500">Returned if an unexpected error occurs while fetching data from the infrastructure or cache layers.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PizzaSize>))]
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