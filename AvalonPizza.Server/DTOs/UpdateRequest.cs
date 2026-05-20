using System.ComponentModel.DataAnnotations;

namespace AvalonPizza.Server.DTOs;

/// <summary>
/// Data transfer object used to update pizza order.
/// </summary>
public class UpdateRequest
{
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
