using System.ComponentModel.DataAnnotations;

namespace AvalonPizza.Server.DTOs;

/// <summary>
/// Data transfer object used to submit a new pizza or update order into the processing system.
/// </summary>
public class OrderRequest
{
    /// <summary>
    /// The full name of the customer placing the order.
    /// </summary>
    /// <example>Jane Doe</example>
    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// The primary contact phone number for delivery confirmation.
    /// </summary>
    /// <example>555-0199</example>
    [Required(ErrorMessage = "Customer phone number is required.")]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// The physical street address where the order will be dispatched.
    /// </summary>
    /// <example>123 Pepperoni Lane, FlavorTown</example>
    [Required(ErrorMessage = "Customer delivery address is required.")]
    [StringLength(255)]
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>
    /// The unique database identifier linking this order to a predefined Pizza Size.
    /// </summary>
    /// <example>2</example>
    [Required(ErrorMessage = "Size id is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid, Pizza Size ID must be provided.")]
    public int SizeId { get; set; }

    /// <summary>
    /// A collection of unique database identifiers for the toppings selected for this pizza.
    /// </summary>
    /// <example>[1, 4, 7]</example>
    [Required]
    [MinLength(1, ErrorMessage = "An order must contain at least one topping.")]
    public List<int> Toppings { get; set; } = new();
}
