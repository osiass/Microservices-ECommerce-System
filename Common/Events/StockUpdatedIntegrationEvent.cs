namespace Common.Events;

public class StockUpdatedIntegrationEvent : IntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public int NewStock { get; set; }
}
