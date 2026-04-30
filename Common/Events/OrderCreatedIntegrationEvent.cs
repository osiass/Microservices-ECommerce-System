namespace Common.Events;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public List<OrderItemStockData> Items { get; set; } = new();
}
