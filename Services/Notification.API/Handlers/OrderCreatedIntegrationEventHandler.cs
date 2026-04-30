using Common.EventBus;
using Common.Events;
using Notification.API.Services;

namespace Notification.API.Handlers;

public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly EmailService _emailService;
    private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;

    public OrderCreatedIntegrationEventHandler(EmailService emailService, ILogger<OrderCreatedIntegrationEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedIntegrationEvent @event)
    {
        _logger.LogInformation("[Notification] Sipariş bildirimi alındı. OrderId: {OrderId}, User: {User}", @event.OrderId, @event.UserName);

        if (string.IsNullOrEmpty(@event.UserEmail))
        {
            _logger.LogWarning("[Notification] Email adresi boş, mail gönderilmedi.");
            return;
        }

        await _emailService.SendOrderConfirmationAsync(@event.UserEmail, @event.UserName, @event.OrderId, @event.TotalPrice);
    }
}
