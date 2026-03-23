namespace Common.DTOs;

public class ShoppingCartDto
{
    public string UserName { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = new();
}