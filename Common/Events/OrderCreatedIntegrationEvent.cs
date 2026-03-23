using Common.EventBus;

namespace Common.Events;

public class OrderCreatedIntegrationEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<OrderItemStockData> Items { get; set; } = new();
}

public class OrderItemStockData
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
