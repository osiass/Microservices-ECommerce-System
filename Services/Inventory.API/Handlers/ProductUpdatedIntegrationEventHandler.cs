using Common.EventBus;
using Common.Events;
using Inventory.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Handlers;

public class ProductUpdatedIntegrationEventHandler : IIntegrationEventHandler<ProductUpdatedIntegrationEvent>
{
    private readonly InventoryContext _context;
    private readonly ILogger<ProductUpdatedIntegrationEventHandler> _logger;

    public ProductUpdatedIntegrationEventHandler(InventoryContext context, ILogger<ProductUpdatedIntegrationEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(ProductUpdatedIntegrationEvent @event)
    {
        _logger.LogInformation("[Inventory.API] Ürün güncelleme olayı alındı: {ProductId} - Yeni İsim: {NewName}", @event.ProductId, @event.NewName);

        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == @event.ProductId);
        
        if (stock != null)
        {
            stock.ProductName = @event.NewName;
            await _context.SaveChangesAsync();
            _logger.LogInformation("[Inventory.API] Ürün stok kaydı güncellendi: {ProductId}", @event.ProductId);
        }
    }
}
