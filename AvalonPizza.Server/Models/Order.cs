using System.ComponentModel.DataAnnotations;

namespace AvalonPizza.Server.Models;

/// <summary>
/// Represents the complete details and lifecycle state of a pizza order.
/// </summary>
public class Order
{
    /// <summary>
    /// The unique database primary key tracking identifier for the order.
    /// </summary>
    /// <example>1024</example>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The full name of the customer who placed the order.
    /// </summary>
    /// <example>Jane Doe</example>
    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// The primary contact phone number for delivery tracking and updates.
    /// </summary>
    /// <example>555-0199</example>
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// The physical destination address where the pizza will be delivered.
    /// </summary>
    /// <example>123 Pepperoni Lane, FlavorTown</example>
    [Required]
    [StringLength(255)]
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>
    /// The unique relational key identifier linking this order to its chosen Pizza Size configuration.
    /// </summary>
    /// <example>3</example>
    [Required]
    [Range(1, int.MaxValue)]
    public int? SizeId { get; set; }

    /// <summary>
    /// The total compounded cost of the order, calculated from the base size price plus all chosen toppings.
    /// </summary>
    /// <example>18.49</example>
    [Required]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// The current stage of the order in the kitchen and delivery workflow.
    /// </summary>
    /// <example>Baking</example>
    [Required]
    public OrderStatus StatusId { get; set; }

    /// <summary>
    /// Soft-delete flag indicating whether this order record is visible and active within operational dashboards.
    /// </summary>
    /// <example>true</example>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    /// The precise timestamp when the order was successfully created and injected into the database.
    /// </summary>
    /// <example>2026-05-18T14:20:00Z</example>
    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The timestamp tracking when the order's state or fields were last modified. Returns null if never updated.
    /// </summary>
    /// <example>2026-05-18T14:35:12Z</example>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// The collection of individual topping entities applied to this pizza order.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<Topping> Toppings { get; set; } = new List<Topping>();
}