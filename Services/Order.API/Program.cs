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

if (args.Contains("--migrate"))
{
    builder.Services.AddDbContext<OrderContext>(o =>
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    var migrateApp = builder.Build();
    using var scope = migrateApp.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrderContext>();
    for (int i = 1; i <= 20; i++)
    {
        try
        {
            await db.Database.MigrateAsync();
            Console.WriteLine("[Order.API] Migration OK.");
            return;
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            var pending = await db.Database.GetPendingMigrationsAsync();
            foreach (var m in pending)
                await db.Database.ExecuteSqlRawAsync($"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{m}', '10.0.5') ON CONFLICT DO NOTHING");
            Console.WriteLine("[Order.API] Migration history güncellendi.");
            return;
        }
        catch (Exception ex) { Console.WriteLine($"[Order.API] Migration {i}/20: {ex.Message}"); if (i == 20) { Environment.Exit(1); } await Task.Delay(3000); }
    }
    return;
}

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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OrderContext>();
    for (int attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("[Order.API] Migration tamamlandı.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Order.API] Migration denemesi {attempt}/10 başarısız: {ex.Message}");
            if (attempt == 10) Console.WriteLine("[Order.API] Migration başarısız, devam ediliyor.");
            else await Task.Delay(3000);
        }
    }
}

app.Run();
