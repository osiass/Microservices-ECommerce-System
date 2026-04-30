using Basket.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Common.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

if (args.Contains("--migrate"))
{
    builder.Services.AddDbContext<BasketContext>(o =>
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    var migrateApp = builder.Build();
    using var scope = migrateApp.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BasketContext>();
    for (int i = 1; i <= 20; i++)
    {
        try
        {
            await db.Database.MigrateAsync();
            Console.WriteLine("[Basket.API] Migration OK.");
            return;
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            Console.WriteLine("[Basket.API] Tablolar mevcut, migration history güncelleniyor...");
            var pending = await db.Database.GetPendingMigrationsAsync();
            foreach (var m in pending)
                await db.Database.ExecuteSqlRawAsync($"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{m}', '10.0.5') ON CONFLICT DO NOTHING");
            Console.WriteLine("[Basket.API] Migration history güncellendi.");
            return;
        }
        catch (Exception ex) { Console.WriteLine($"[Basket.API] Migration {i}/20: {ex.Message}"); if (i == 20) { Environment.Exit(1); } await Task.Delay(3000); }
    }
    return;
}

// External Service Clients
builder.Services.AddHttpClient("Catalog", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Catalog"]!);
    client.Timeout = TimeSpan.FromSeconds(5); // İç çağrılar uzun sürmemeli
});

builder.Services.AddHttpClient("Discount", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Discount"]!);
    client.Timeout = TimeSpan.FromSeconds(3); // İndirim servisi hızlı cevap vermeli
});

// Database & API Configuration
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BasketContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
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
    });

builder.Services.AddAuthorization();

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
    var context = scope.ServiceProvider.GetRequiredService<BasketContext>();
    for (int attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("[Basket.API] Migration tamamlandı.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Basket.API] Migration denemesi {attempt}/10 başarısız: {ex.Message}");
            if (attempt == 10) Console.WriteLine("[Basket.API] Migration başarısız, devam ediliyor.");
            else await Task.Delay(3000);
        }
    }
}

app.Run();
