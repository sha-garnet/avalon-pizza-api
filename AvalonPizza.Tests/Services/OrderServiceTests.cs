using AutoMapper;
using AvalonPizza.Server.Interfaces.Repositories;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using AvalonPizza.Server.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AvalonPizza.Tests.Services;

public class OrderServiceTests
{
    [Fact]
    public async Task GetOrderDetailsAsync_ReturnsPopulatedOrder_WhenOrderExists()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        var mockToppingService = new Mock<IToppingService>();
        var mockSizeService = new Mock<IPizzaSizeService>();
        var mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<OrderService>>();

        int targetOrderId = 101;
        var fakeOrder = new Order
        {
            Id = targetOrderId,
            CustomerName = "John Doe",
            PhoneNumber = "555-0199",
            DeliveryAddress = "456 Pizza Plaza",
            SizeId = 1,
            TotalPrice = 18.49m,
            StatusId = OrderStatus.Pending,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            Toppings = new List<Topping>
            {
                new() { ToppingId = 1, ToppingName = "Pepperoni", ToppingPrice = 1.75m, IsActive = true }
            }
        };

        mockRepo.Setup(r => r.GetOrderByIdAsync(targetOrderId))
                .ReturnsAsync(fakeOrder);

        var service = new OrderService(
            mockRepo.Object,
            mockToppingService.Object,
            mockSizeService.Object,
            mockMapper.Object,
            mockLogger.Object
        );

        // Act
        var result = await service.GetOrderDetailsAsync(targetOrderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetOrderId, result.Id);
        Assert.Equal("John Doe", result.CustomerName);
        Assert.Single(result.Toppings);
        mockRepo.Verify(r => r.GetOrderByIdAsync(targetOrderId), Times.Once);
    }

    [Fact(Skip = "TODO: Test that PlaceOrderAsync correctly maps fields, runs price calculations, and passes data to repository.")]
    public void PlaceOrderAsync_CalculatesPriceAndPersistsOrder_Placeholder()
    {
        // Future implementation placeholder
    }

    [Fact(Skip = "TODO: Test that UpdateOrderAsync executes recalculated changes if status configuration is Pending.")]
    public void UpdateOrderAsync_RecalculatesPriceAndUpdatesRecord_Placeholder()
    {
        // Future implementation placeholder
    }

    [Fact(Skip = "TODO: Test that UpdateOrderStatusAsync advances lifecycle flags correctly down-stack.")]
    public void UpdateOrderStatusAsync_DispatchesStatusChangeToRepo_Placeholder()
    {
        // Future implementation placeholder
    }

    [Fact(Skip = "TODO: Test that DeleteOrderAsync properly marks data tracking entities as soft-deleted.")]
    public void DeleteOrderAsync_CoordinatesSoftDeletionSequence_Placeholder()
    {
        // Future implementation placeholder
    }
}
