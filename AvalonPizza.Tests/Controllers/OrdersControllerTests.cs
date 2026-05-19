using AvalonPizza.Server.Controllers;
using AvalonPizza.Server.Interfaces.Services;
using AvalonPizza.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AvalonPizza.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly Mock<ILogger<OrdersController>> _mockLogger;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _mockLogger = new Mock<ILogger<OrdersController>>();
        _controller = new OrdersController(_mockOrderService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetOrder_Returns200Ok_WithExpectedOrderDetails()
    {
        // Arrange
        int targetOrderId = 42;
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

        _mockOrderService.Setup(s => s.GetOrderDetailsAsync(targetOrderId))
                             .ReturnsAsync(fakeOrder);

        // Act
        var actionResult = await _controller.GetOrder(targetOrderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(200, okResult.StatusCode);

        var returnedOrder = Assert.IsType<Order>(okResult.Value);
        Assert.Equal(targetOrderId, returnedOrder.Id);
        Assert.Equal("John Doe", returnedOrder.CustomerName);
        Assert.Equal("555-0199", returnedOrder.PhoneNumber);
        Assert.Equal(18.49m, returnedOrder.TotalPrice);
        Assert.Single(returnedOrder.Toppings);
        Assert.Equal("Pepperoni", returnedOrder.Toppings[0].ToppingName);
    }

    [Fact(Skip = "TODO: Test that CreateOrder returns 201 Created with valid payload and updates route path.")]
    public void CreateOrder_Returns201Created_WithValidPayload_Placeholder()
    {
        // Future implementation placeholder
    }

    [Fact(Skip = "TODO: Test that UpdateOrder returns 204 NoContent upon structural pizza adjustment success.")]
    public void UpdateOrder_Returns204NoContent_OnSuccessfulModification_Placeholder()
    {
        // Future implementation placeholder
    }

    [Fact(Skip = "TODO: Test administrative role routing validations and state transitions for UpdateStatus.")]
    public void UpdateStatus_Returns204NoContent_OnLegalStateTransition_Placeholder()
    {
        // Future implementation placeholder
    }

    [Fact(Skip = "TODO: Test that DeleteOrder processes soft-deletions safely when StatusId is Pending.")]
    public void DeleteOrder_ExecutesSoftDelete_WhenOrderIsPending_Placeholder()
    {
        // Future implementation placeholder
    }
}