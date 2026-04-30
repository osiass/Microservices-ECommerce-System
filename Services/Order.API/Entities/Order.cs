using Common.DTOs;

namespace Order.API.Entities;

public class Order
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string AddressLine { get; set; } = string.Empty; 
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow; 
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? CouponCode { get; set; }
    public decimal Discount { get; set; }
    public string? TransactionId { get; set; }

    // Bir siparişte birden fazla ürün olabilir 
    public List<OrderItem> OrderItems { get; set; } = new();
}