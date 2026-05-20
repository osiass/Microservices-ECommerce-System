namespace Basket.API.Entities;

public class ShoppingCart
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<BasketItem> Items { get; set; } = new();
    public decimal Discount { get; set; }
    public string? CouponCode { get; set; }
}
