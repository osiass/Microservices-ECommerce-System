using Common.EventBus;
using Common.Events;
using Catalog.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Handlers;

public class StockUpdatedIntegrationEventHandler : IIntegrationEventHandler<StockUpdatedIntegrationEvent>
{
    private readonly CatalogContext _context;
    private readonly ILogger<StockUpdatedIntegrationEventHandler> _logger;

    public StockUpdatedIntegrationEventHandler(CatalogContext context, ILogger<StockUpdatedIntegrationEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(StockUpdatedIntegrationEvent @event)
    {
        _logger.LogInformation("Stok güncelleme eventi alındı. Ürün: {ProductId}, Yeni Stok: {NewStock}", @event.ProductId, @event.NewStock);

        if (Guid.TryParse(@event.ProductId, out Guid productId))
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.StockQuantity = @event.NewStock;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Katalog servisinde ürün {ProductId} stoğu {NewStock} olarak güncellendi.", @event.ProductId, @event.NewStock);
            }
            else
            {
                _logger.LogWarning("Katalog servisinde ürün {ProductId} bulunamadı!", @event.ProductId);
            }
        }
        else
        {
            _logger.LogError("Geçersiz ProductId formatı: {ProductId}", @event.ProductId);
        }
    }
}
