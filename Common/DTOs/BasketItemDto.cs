namespace Common.DTOs;

public class BasketItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; } // İndirimsiz fiyat
    public int Quantity { get; set; }
}