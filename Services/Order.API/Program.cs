using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Order.API.Data;
using System.Text;
using System;
using Common.EventBus;
using RabbitMQ.Client;
using Common.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add HttpClient for connecting to Payment-API via Aspire service discovery
builder.Services.AddHttpClient("payment-api", client =>
{
    client.BaseAddress = new Uri("https+http://payment-api");
});

builder.Services.AddDbContext<OrderContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Aspire RabbitMQ Client ve IEventBus kaydı
builder.AddRabbitMQClient("messaging");
builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

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

// Veritabanını otomatik oluştur ve migrasyonları uygula
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var context = scope.ServiceProvider.GetRequiredService<OrderContext>();
        
        await context.Database.MigrateAsync();
        
        Console.WriteLine("[Order.API] Migrasyonlar başarıyla uygulandı ve veritabanı güncellendi.");
    }
    catch (Exception ex)
    {
        var fullError = ex.Message + (ex.InnerException != null ? (" | Inner: " + ex.InnerException.Message) : "");
        Console.WriteLine($"[Order.API] Startup error: {fullError}");
    }
}

app.Run();
