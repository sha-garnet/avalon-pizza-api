using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace AvalonPizza.Server.Controllers;

/// <summary>
/// /api/toppings
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ToppingsController : ControllerBase
{
    private readonly IToppingService _toppingService;
    private readonly ILogger<ToppingsController> _logger;

    public ToppingsController(IToppingService toppingService, ILogger<ToppingsController> logger)
    {
        _toppingService = toppingService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Topping>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Topping>>> GetAll()
    {
        try
        {
            var toppings = await _toppingService.GetAllToppingsAsync();
            return Ok(toppings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching toppings.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving data.");
        }
    }
}