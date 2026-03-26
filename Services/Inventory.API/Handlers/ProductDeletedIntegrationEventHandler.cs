using Common.EventBus;
using Common.Events;
using Inventory.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Handlers;

public class ProductDeletedIntegrationEventHandler : IIntegrationEventHandler<ProductDeletedIntegrationEvent>
{
    private readonly InventoryContext _context;
    private readonly ILogger<ProductDeletedIntegrationEventHandler> _logger;

    public ProductDeletedIntegrationEventHandler(InventoryContext context, ILogger<ProductDeletedIntegrationEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(ProductDeletedIntegrationEvent @event)
    {
        _logger.LogInformation("[Inventory.API] Ürün silme olayı alındı: {ProductId}", @event.ProductId);

        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == @event.ProductId);
        
        if (stock != null)
        {
            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[Inventory.API] Ürün stok kaydı silindi: {ProductId}", @event.ProductId);
        }
    }
}
