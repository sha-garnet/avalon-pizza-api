using Microsoft.AspNetCore.Mvc;

namespace AvalonPizza.Server.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: api/greet
public class GreetController : ControllerBase
{
    // This responds to a GET request
    [HttpGet]
    public IActionResult GetGreeting()
    {
        return Ok(new { message = "Hello from your first Controller!" });
    }

    // This responds to a GET request with a parameter: api/greet/James
    [HttpGet("{name}")]
    public IActionResult GetPersonalGreeting(string name)
    {
        return Ok(new { message = $"Hello, {name}! Your API controller is working." });
    }
}