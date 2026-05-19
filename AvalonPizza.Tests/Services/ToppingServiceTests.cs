using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Models;
using AvalonPizza.Server.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AvalonPizza.Tests.Services;

public class ToppingServiceTests
{
    [Fact]
    public async Task GetAllToppingsAsync_ReturnsCachedData_OnCacheHit()
    {
        // Arrange
        var mockRepo = new Mock<IToppingRepository>();
        var mockCache = new Mock<ICacheService>();
        var mockLogger = new Mock<ILogger<ToppingService>>();
        var service = new ToppingService(mockRepo.Object, mockCache.Object, mockLogger.Object);

        var fakeToppings = new List<Topping>
        {
            new() { ToppingId = 1, ToppingName = "Pepperoni", ToppingPrice = 1.75m, IsActive = true },
            new() { ToppingId = 2, ToppingName = "Mushrooms", ToppingPrice = 1.25m, IsActive = true }
        };

        mockCache.Setup(c => c.GetAsync<IEnumerable<Topping>>("pizza:toppings:all"))
                  .ReturnsAsync(fakeToppings);

        // Act
        var result = await service.GetAllToppingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        mockRepo.Verify(r => r.GetAllToppingsAsync(), Times.Never);
    }
}
