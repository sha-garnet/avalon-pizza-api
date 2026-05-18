using System.ComponentModel.DataAnnotations;

namespace AvalonPizza.Server.Models;

/// <summary>
/// Represents a pizza topping configuration available in the catalog.
/// </summary>
public class Topping
{
    /// <summary>
    /// The unique database primary key identifier for the topping.
    /// </summary>
    /// <example>1</example>
    [Key]
    public int ToppingId { get; set; }

    /// <summary>
    /// The display name of the pizza topping.
    /// </summary>
    /// <example>Pepperoni</example>
    [Required]
    [StringLength(50)]
    public string ToppingName { get; set; } = string.Empty;

    /// <summary>
    /// The additional retail cost added to the base price of the pizza for this topping.
    /// </summary>
    /// <example>1.75</example>
    [Required]
    public decimal ToppingPrice { get; set; }

    /// <summary>
    /// Indicates whether the topping is currently active, in-stock, and available for customer orders.
    /// </summary>
    /// <example>true</example>
    [Required]
    public bool IsActive { get; set; }
}