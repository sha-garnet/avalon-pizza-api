using System.ComponentModel.DataAnnotations;

namespace AvalonPizza.Server.Models;

public class PizzaOrder
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Size is required.")]
    [RegularExpression("Small|Medium|Large", ErrorMessage = "Size must be Small, Medium, or Large.")]
    public string Size { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "You must select at least one topping.")]
    [MaxLength(5, ErrorMessage = "Maximum 5 toppings allowed.")]
    public List<string> Toppings { get; set; } = new();

    public decimal Price { get; set; }
}
