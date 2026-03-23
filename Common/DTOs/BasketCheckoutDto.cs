namespace Common.DTOs;

public class BasketCheckoutDto
{
    public string UserName { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public int ExpirationMonth { get; set; }
    public int ExpirationYear { get; set; }
    public int CVV { get; set; }
    public List<BasketItemDto> Items { get; set; } = new();
}