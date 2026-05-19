using AvalonPizza.Server.Controllers;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AvalonPizza.Tests.Controllers;

public class PizzaSizesControllerTests
{
    [Fact]
    public async Task GetAll_Returns200Ok_WhenSizesExist()
    {
        // Arrange
        var mockService = new Mock<IPizzaSizeService>();
        var mockLogger = new Mock<ILogger<PizzaSizesController>>();
        var controller = new PizzaSizesController(mockService.Object, mockLogger.Object);

        var fakeSizes = new List<PizzaSize>
        {
            new() { SizeId = 1, SizeName = "Small", BasePrice = 8.99m, IsActive = true },
            new() { SizeId = 2, SizeName = "Large", BasePrice = 14.99m, IsActive = true }
        };

        mockService.Setup(s => s.GetAllSizesAsync())
                    .ReturnsAsync(fakeSizes);

        // Act
        var actionResult = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var returnedSizes = Assert.IsAssignableFrom<IEnumerable<PizzaSize>>(okResult.Value).ToList();
        Assert.Equal(2, returnedSizes.Count);
        Assert.Equal("Small", returnedSizes[0].SizeName);
        Assert.Equal(8.99m, returnedSizes[0].BasePrice);
        Assert.Equal("Large", returnedSizes[1].SizeName);
        Assert.Equal(14.99m, returnedSizes[1].BasePrice);
    }
}