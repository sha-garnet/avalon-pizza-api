using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.DTOs;

/// <summary>
/// Data transfer object used to request an administrative update to an order's lifecycle state.
/// </summary>
public class StatusRequest
{
    /// <summary>
    /// The target lifecycle state to assign to the order.
    /// </summary>
    /// <example>3</example>
    public OrderStatus Status { get; set; }
}