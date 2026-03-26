using Common.EventBus;

namespace Common.Events;

public class ProductDeletedIntegrationEvent : IntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;

    public ProductDeletedIntegrationEvent(string productId)
    {
        ProductId = productId;
    }
}
