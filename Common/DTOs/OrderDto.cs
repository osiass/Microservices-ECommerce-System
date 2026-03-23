namespace Common.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}
