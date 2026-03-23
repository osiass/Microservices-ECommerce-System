namespace Basket.API.Entities;

public class ShoppingCart
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty; // Hangi kullanıcının sepeti
    public List<BasketItem> Items { get; set; } = new(); // Sepetteki ürünler
}
