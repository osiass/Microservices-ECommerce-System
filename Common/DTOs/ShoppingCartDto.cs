namespace Common.Dtos;

public class ShoppingCartDto
{
    public string UserName { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = new();
}