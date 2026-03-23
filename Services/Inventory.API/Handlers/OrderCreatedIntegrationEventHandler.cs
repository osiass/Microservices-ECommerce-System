using Common.EventBus;
using Common.Events;
using Inventory.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Handlers;

public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly InventoryContext _context;
    private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;

    public OrderCreatedIntegrationEventHandler(InventoryContext context, ILogger<OrderCreatedIntegrationEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedIntegrationEvent @event)
    {
        _logger.LogInformation("Yeni sipariş için stok düşme işlemi başladı. Sipariş No: {OrderId}", @event.OrderId);

        //Gelen event içindeki ürün listesini dön
        foreach (var item in @event.Items)
        {
            //Veritabanındaki stok kaydını ProductIdye göre ara
            var stock = await _context.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

            if (stock != null)
            {
                //Bulduğumuz kayıttan sipariş edilen adet kadar düş
                stock.Count -= item.Quantity;
                _logger.LogInformation("Ürün {ProductId} için stok {Quantity} adet düşürüldü. Yeni stok: {NewCount}", item.ProductId, item.Quantity, stock.Count);
            }
            else
            {
                _logger.LogWarning("Ürün {ProductId} için stok kaydı bulunamadı!", item.ProductId);
            }
        }

        //Tüm değişiklikleri veritabanına tek seferde yansıt
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Sipariş {OrderId} için stok güncelleme tamamlandı.", @event.OrderId);
    }
}
