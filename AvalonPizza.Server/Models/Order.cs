using System.ComponentModel.DataAnnotations;

namespace AvalonPizza.Server.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer phone number is required.")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer delivery address is required.")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Size id is required.")]
        public int? SizeId { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus StatusId { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "You must select at least one topping.")]
        public List<Topping> Toppings { get; set; } = new List<Topping>();
    }
}