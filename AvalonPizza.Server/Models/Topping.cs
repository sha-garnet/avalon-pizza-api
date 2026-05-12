namespace AvalonPizza.Server.Models
{
    public class Topping
    {
        public int ToppingId { get; set; }
        public string ToppingName { get; set; } = string.Empty;
        public decimal ToppingPrice { get; set; }
        public bool IsActive { get; set; }
    }
}