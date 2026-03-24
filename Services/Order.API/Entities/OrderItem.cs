namespace Order.API.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public int OrderId { get; set; } // Hangi siparişe ait olduğunu bilmek için (Foreign Key)
}