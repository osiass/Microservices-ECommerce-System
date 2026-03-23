namespace Inventory.API.Entities;

public class Stock
{
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Count { get; set; }
}
