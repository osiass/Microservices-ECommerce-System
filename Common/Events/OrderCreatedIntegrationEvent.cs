namespace Common.Events;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<OrderItemStockData> Items { get; set; } = new();
}
