using System.ComponentModel.DataAnnotations;

namespace AvalonPizza.Server.DTOs;

public class OrderRequest
{
    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer phone number is required.")]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer delivery address is required.")]
    [StringLength(255)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Size id is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid, Pizza Size ID must be provided.")]
    public int SizeId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "An order must contain at least one topping.")]
    public List<int> Toppings { get; set; } = new();
}
