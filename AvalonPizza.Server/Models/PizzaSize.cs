namespace AvalonPizza.Server.Models;

public class PizzaSize
{
    public int SizeId { get; set; }
    public string SizeName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
}
