using AvalonPizza.Server.Controllers;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AvalonPizza.Tests.Controllers;

public class ToppingsControllerTests
{
    private readonly Mock<IToppingService> _mockService;
    private readonly Mock<ILogger<ToppingsController>> _mockLogger;
    private readonly ToppingsController _controller;
    
    public ToppingsControllerTests()
    {
        _mockService = new Mock<IToppingService>();
        _mockLogger = new Mock<ILogger<ToppingsController>>();
        _controller = new ToppingsController(_mockService.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task GetAll_Returns200Ok_WhenToppingsExist()
    {
        // Arrange
        var fakeToppings = new List<Topping>
        {
            new() { ToppingId = 1, ToppingName = "Pepperoni", ToppingPrice = 1.75m, IsActive = true },
            new() { ToppingId = 2, ToppingName = "Mushrooms", ToppingPrice = 1.25m, IsActive = true }
        };

        _mockService.Setup(s => s.GetAllToppingsAsync())
                   .ReturnsAsync(fakeToppings);

        // Act
        var actionResult = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var returnedToppings = Assert.IsAssignableFrom<IEnumerable<Topping>>(okResult.Value);
        Assert.Equal(2, returnedToppings.Count());
        Assert.Equal("Pepperoni", returnedToppings.ElementAt(0).ToppingName);
        Assert.Equal("Mushrooms", returnedToppings.ElementAt(1).ToppingName);
    }
}
