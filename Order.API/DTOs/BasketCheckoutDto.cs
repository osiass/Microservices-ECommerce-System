namespace Order.API.DTOs
{
    public class BasketCheckoutDto
    {
        public string UserName { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public List<BasketItemDto> Items { get; set; } = new();
    }
}
