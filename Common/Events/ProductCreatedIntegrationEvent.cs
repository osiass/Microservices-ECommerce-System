using Common.EventBus;

namespace Common.Events;

public class ProductCreatedIntegrationEvent : IntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int InitialStock { get; set; }

    public ProductCreatedIntegrationEvent(string productId, string name, int initialStock)
    {
        ProductId = productId;
        Name = name;
        InitialStock = initialStock;
    }
}
