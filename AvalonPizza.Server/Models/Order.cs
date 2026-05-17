using System.ComponentModel.DataAnnotations;

namespace AvalonPizza.Server.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int? SizeId { get; set; }

    public decimal TotalPrice { get; set; }
    public OrderStatus StatusId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    [Required]
    [MinLength(1)]
    public List<Topping> Toppings { get; set; } = new List<Topping>();
}