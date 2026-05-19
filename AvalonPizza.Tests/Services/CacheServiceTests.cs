using AvalonPizza.Server.Models;
using AvalonPizza.Server.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;

namespace AvalonPizza.Tests.Services;



public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<CacheService>> _mockLogger;
    private readonly CacheService _cacheService;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public CacheServiceTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<CacheService>>();
        _cacheService = new CacheService(_mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsDeserializedObject_OnCacheHit()
    {
        // Arrange
        string targetKey = "pizza:sizes:all";
        var expectedSize = new PizzaSize { SizeId = 1, SizeName = "Small", BasePrice = 8.99m, IsActive = true };
        string jsonString = JsonSerializer.Serialize(expectedSize, JsonOptions);
        byte[] encodedBytes = Encoding.UTF8.GetBytes(jsonString); // GetAsync (returns a byte[])

        // IDistributedCache.GetStringAsync used in service (extension method that can't be mocked) internally calls GetAsync(key, cancellationToken)
        _mockCache
            .Setup(c => c.GetAsync(targetKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(encodedBytes);

        // Act
        var result = await _cacheService.GetAsync<PizzaSize>(targetKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Small", result.SizeName);
        Assert.Equal(8.99m, result.BasePrice);
        _mockCache.Verify(c => c.GetAsync(targetKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_SerializesAndStoresStringWithCorrectExpiration_OnInvocation()
    {
        // Arrange
        string targetKey = "pizza:toppings:all";
        var toppingToCache = new Topping { ToppingId = 2, ToppingName = "Mushrooms", ToppingPrice = 1.25m, IsActive = true };
        TimeSpan absoluteExpiration = TimeSpan.FromHours(6);

        _mockCache
            .Setup(c => c.SetAsync(
                targetKey,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.SetAsync(targetKey, toppingToCache, absoluteExpiration);

        // Assert
        _mockCache.Verify(c => c.SetAsync(
            targetKey,
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(opts =>
                opts.AbsoluteExpirationRelativeToNow == absoluteExpiration &&
                opts.SlidingExpiration == TimeSpan.FromHours(4)), // Matches your >4 hour calculation rule
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_InvokesCacheEviction_OnInvocation()
    {
        // Arrange
        string targetKey = "pizza:sizes:all";

        _mockCache
            .Setup(c => c.RemoveAsync(targetKey, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.RemoveAsync(targetKey);

        // Assert
        _mockCache.Verify(c => c.RemoveAsync(targetKey, It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(Skip = "TODO: Test that GetAsync logs an error and returns default when a malformed JsonException occurs.")]
    public void GetAsync_HandlesJsonSerializationFailures_Placeholder()
    {
        // Future implementation placeholder
    }

    [Fact(Skip = "TODO: Test that GetAsync catches general Redis connection exceptions cleanly without breaking application state.")]
    public void GetAsync_CatchesGeneralExceptions_ReturnsDefault_Placeholder()
    {
        // Future implementation placeholder
    }

    [Fact(Skip = "TODO: Test that SetAsync catches error flags safely if the underlying server connection times out.")]
    public void SetAsync_LogsErrorOnCacheWriteFailure_Placeholder()
    {
        // Future implementation placeholder
    }
}