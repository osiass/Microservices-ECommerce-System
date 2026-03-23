using Inventory.API.Data;
using Inventory.API.Handlers;
using Common.EventBus;
using Common.Events;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// PostgreSQL Kaydı
builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Aspire RabbitMQ Client ve IEventBus kaydı
builder.AddRabbitMQClient("messaging");
builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();

// Handler Kaydı
builder.Services.AddScoped<OrderCreatedIntegrationEventHandler>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// EVENT BUS SUBSCRIPTION (DINLEME):
// Uygulama ayağa kalktığında RabbitMQ'daki "OrderCreatedIntegrationEvent" kuyruğunu dinlemeye başlıyoruz.
var eventBus = app.Services.GetRequiredService<IEventBus>();
await eventBus.SubscribeAsync<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

app.Run();
