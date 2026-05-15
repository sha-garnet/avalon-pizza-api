namespace AvalonPizza.Server.Models;

public enum OrderStatus
{
    Pending = 1,
    InPreparation = 2,
    Baking = 3,
    OutForDelivery = 4,
    Delivered = 5,
    Cancelled = 6
}
