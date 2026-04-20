using Catalog.API.Data;
using Catalog.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Veritabanı servisi
builder.Services.AddDbContext<CatalogContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Authentication Konfigürasyonu Identityden gelen tokenları okumak için
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Geliştirme ortamı için pasif
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
                // Sorunu anlamak için hata mesajını headera ekle
                context.Response.Headers.Append("X-Auth-Error", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context => 
            {
                // Ham header kontrolü (Typed değil, direkt string)
                var rawHeader = context.Request.Headers["Authorization"].ToString();
                var allHeaderKeys = string.Join(",", context.Request.Headers.Keys);
                
                // Tüm gelen başlıkları birleştirip yeni bir başlık olarak ekle
                var allHeadersString = string.Join("; ", context.Request.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}"));
                context.Response.Headers.Append("X-Debug-All-Headers", allHeadersString);

                context.Response.Headers.Append("X-Debug-Header", string.IsNullOrEmpty(rawHeader) ? "MISSING" : "PRESENT");
                context.Response.Headers.Append("X-Debug-Keys", allHeaderKeys);
                context.Response.Headers.Append("X-Auth-Challenge", context.ErrorDescription ?? "Unauthorized");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// EventBus Konfigürasyonu (RabbitMQ)
builder.AddRabbitMQClient("messaging");
builder.Services.AddSingleton<Common.EventBus.IEventBus, Common.EventBus.RabbitMQEventBus>();
builder.Services.AddTransient<Catalog.API.Handlers.StockUpdatedIntegrationEventHandler>();

var app = builder.Build();

app.UseMiddleware<Common.Middleware.GlobalExceptionMiddleware>();

// Eventlere abone ol
var eventBus = app.Services.GetRequiredService<Common.EventBus.IEventBus>();
await eventBus.SubscribeAsync<Common.Events.StockUpdatedIntegrationEvent, Catalog.API.Handlers.StockUpdatedIntegrationEventHandler>();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
    for (int attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("[Catalog.API] Migration tamamlandı.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Catalog.API] Migration denemesi {attempt}/10 başarısız: {ex.Message}");
            if (attempt == 10) Console.WriteLine("[Catalog.API] Migration başarısız, devam ediliyor.");
            else await Task.Delay(3000);
        }
    }
}
app.Run();