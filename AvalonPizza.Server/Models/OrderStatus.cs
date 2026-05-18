namespace AvalonPizza.Server.Models;

/// <summary>
/// Defines the current operational lifecycle state of a pizza order.
/// </summary>
public enum OrderStatus
{
    /// <summary>Order submitted by customer, awaiting store verification.</summary>
    Pending = 1,

    /// <summary>Kitchen staff are assembling ingredients and tossing dough.</summary>
    InPreparation = 2,

    /// <summary>The pizza is actively baking inside the deck oven.</summary>
    Baking = 3,

    /// <summary>The order has been picked up by a driver and is en route.</summary>
    OutForDelivery = 4,

    /// <summary>The order was successfully handed off to the customer.</summary>
    Delivered = 5,

    /// <summary>The order was terminated due to a payment fault or cancellation request.</summary>
    Cancelled = 6
}
