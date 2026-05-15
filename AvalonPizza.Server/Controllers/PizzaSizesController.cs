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
    /// Retrieves all pizza sizes and base prices. Optimized with Redis caching.
    /// GET: api/pizzas/sizes
    /// </summary>
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