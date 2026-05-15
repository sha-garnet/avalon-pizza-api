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
    /// Retrieves all active toppings. Optimized with Redis caching.
    /// GET: api/toppings
    /// </summary>
    /// <returns>IEnumerable<Topping></returns>
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