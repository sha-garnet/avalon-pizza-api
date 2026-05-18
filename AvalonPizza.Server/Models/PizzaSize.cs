using System.ComponentModel.DataAnnotations;

namespace AvalonPizza.Server.Models;

/// <summary>
/// Represents a pizza size option available in the system catalog.
/// </summary>
public class PizzaSize
{
    /// <summary>
    /// The unique database primary key identifier for the pizza size.
    /// </summary>
    /// <example>3</example>
    [Key]
    public int SizeId { get; set; }

    /// <summary>
    /// The descriptive name of the pizza size.
    /// </summary>
    /// <example>Large</example>
    [Required]
    [StringLength(50)]
    public string SizeName { get; set; } = string.Empty;

    /// <summary>
    /// The baseline retail cost of the pizza before any additional toppings are added.
    /// </summary>
    /// <example>14.99</example>
    [Required]
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Indicates whether this pizza size is currently available for customers to order.
    /// </summary>
    /// <example>true</example>
    [Required]
    public bool IsActive { get; set; }
}
