namespace Basket.API.Entities
{
    public class BasketItem
    {
        public int Id { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int ShoppingCartId { get; set; }
    }
}
