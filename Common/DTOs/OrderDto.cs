namespace Common.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedDate { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int StatusCode => (int)Status;
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public string? TransactionId { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}
