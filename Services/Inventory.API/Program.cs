using Inventory.API.Data;
using Inventory.API.Handlers;
using Common.EventBus;
using Common.Events;
using Microsoft.EntityFrameworkCore;
using Common.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// PostgreSQL Kaydı
builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Authentication Konfigürasyonu
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Identity.API",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ECommerce.User",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured. Set the 'Jwt__Key' environment variable."))),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context => 
            {
                context.Response.Headers.Append("X-Auth-Error", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context => 
            {
                context.Response.Headers.Append("X-Auth-Challenge", context.ErrorDescription ?? "Unauthorized");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Aspire RabbitMQ Client ve IEventBus kaydı
builder.AddRabbitMQClient("messaging");
builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();

// Handler Kaydı
builder.Services.AddScoped<OrderCreatedIntegrationEventHandler>();
builder.Services.AddTransient<ProductCreatedIntegrationEventHandler>();
builder.Services.AddTransient<ProductDeletedIntegrationEventHandler>();
builder.Services.AddTransient<ProductUpdatedIntegrationEventHandler>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Veritabanını otomatik oluştur ve stokları ekle
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Inventory.API] Startup error: {ex.Message}");
    }
}

// EVENT BUS SUBSCRIPTION 
var eventBus = app.Services.GetRequiredService<IEventBus>();
await eventBus.SubscribeAsync<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
await eventBus.SubscribeAsync<ProductCreatedIntegrationEvent, ProductCreatedIntegrationEventHandler>();
await eventBus.SubscribeAsync<ProductDeletedIntegrationEvent, ProductDeletedIntegrationEventHandler>();
await eventBus.SubscribeAsync<ProductUpdatedIntegrationEvent, ProductUpdatedIntegrationEventHandler>();

app.Run();
