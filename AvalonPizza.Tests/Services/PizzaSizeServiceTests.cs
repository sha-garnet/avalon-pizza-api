using AvalonPizza.Server.Interfaces;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Models;
using AvalonPizza.Server.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AvalonPizza.Tests.Services;

public class PizzaSizeServiceTests
{
    [Fact]
    public async Task GetAllSizesAsync_ReturnsCachedData_OnCacheHit()
    {
        // Arrange
        var mockRepo = new Mock<IPizzaSizeRepository>();
        var mockCache = new Mock<ICacheService>();
        var mockLogger = new Mock<ILogger<PizzaSizeService>>();
        var service = new PizzaSizeService(mockRepo.Object, mockCache.Object, mockLogger.Object);

        var fakeSizes = new List<PizzaSize>
        {
            new() { SizeId = 1, SizeName = "Small", BasePrice = 9.99m, IsActive = true },
            new() { SizeId = 2, SizeName = "Large", BasePrice = 14.99m, IsActive = true }
        };

        mockCache.Setup(c => c.GetAsync<IEnumerable<PizzaSize>>("pizza:sizes:all"))
                  .ReturnsAsync(fakeSizes);

        // Act
        var result = await service.GetAllSizesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        mockRepo.Verify(r => r.GetAllAsync(), Times.Never);
    }
}
