using Common.EventBus;
using Common.Events;
using Inventory.API.Data;
using Inventory.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Inventory.API.Handlers;

public class ProductCreatedIntegrationEventHandler : IIntegrationEventHandler<ProductCreatedIntegrationEvent>
{
    private readonly InventoryContext _context;
    private readonly ILogger<ProductCreatedIntegrationEventHandler> _logger;

    public ProductCreatedIntegrationEventHandler(InventoryContext context, ILogger<ProductCreatedIntegrationEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(ProductCreatedIntegrationEvent @event)
    {
        _logger.LogInformation("[Inventory.API] Yeni ürün oluşturma olayı alındı: {ProductId} - {ProductName}", @event.ProductId, @event.Name);

        var existingStock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == @event.ProductId);
        
        if (existingStock == null)
        {
            var newStock = new Stock
            {
                ProductId = @event.ProductId,
                ProductName = @event.Name,
                Quantity = @event.InitialStock
            };

            _context.Stocks.Add(newStock);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[Inventory.API] Yeni ürün için stok kaydı oluşturuldu: {ProductId}, Başlangıç Stoğu: {Stock}", @event.ProductId, @event.InitialStock);
        }
        else
        {
            _logger.LogWarning("[Inventory.API] Ürün için stok kaydı zaten mevcut: {ProductId}", @event.ProductId);
        }
    }
}
