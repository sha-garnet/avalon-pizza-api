using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace AvalonPizza.Server.Controllers;

/// <summary>
/// /api/pizzasizes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PizzaSizesController : ControllerBase
{
    private readonly IPizzaSizeService _pizzaSizeService;
    private readonly ILogger<PizzaSizesController> _logger;

    public PizzaSizesController(IPizzaSizeService pizzaSizeService, ILogger<PizzaSizesController> logger)
    {
        _pizzaSizeService = pizzaSizeService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PizzaSize>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PizzaSize>>> GetAll()
    {
        try
        {
            var sizes = await _pizzaSizeService.GetAllSizesAsync();
            return Ok(sizes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching pizza sizes.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving data.");
        }
    }
}