using Notification.API.Handlers;
using Notification.API.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.AddRabbitMQClient("messaging");
builder.Services.AddSingleton<Common.EventBus.IEventBus, Common.EventBus.RabbitMQEventBus>();
builder.Services.AddTransient<OrderCreatedIntegrationEventHandler>();
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();
app.MapDefaultEndpoints();

var eventBus = app.Services.GetRequiredService<Common.EventBus.IEventBus>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

for (int i = 1; i <= 10; i++)
{
    try
    {
        await eventBus.SubscribeAsync<Common.Events.OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
        logger.LogInformation("[Notification.API] RabbitMQ'ya abone olundu.");
        break;
    }
    catch (Exception ex)
    {
        logger.LogWarning("[Notification.API] RabbitMQ bağlantı denemesi {Attempt}/10: {Message}", i, ex.Message);
        if (i == 10) logger.LogError("[Notification.API] RabbitMQ bağlantısı kurulamadı.");
        else await Task.Delay(3000);
    }
}

app.Run();
