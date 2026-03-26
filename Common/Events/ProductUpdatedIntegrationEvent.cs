using Common.EventBus;

namespace Common.Events;

public class ProductUpdatedIntegrationEvent : IntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;

    public ProductUpdatedIntegrationEvent(string productId, string newName)
    {
        ProductId = productId;
        NewName = newName;
    }
}
