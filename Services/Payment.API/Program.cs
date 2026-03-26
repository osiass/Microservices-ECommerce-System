using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Payment.API.Data;
using Common.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults(); // Aspire telemetry and health checks

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PaymentContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapDefaultEndpoints(); // Aspire health endpoints mapping

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Veritabanını otomatik oluştur ve migrasyonları uygula
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var context = scope.ServiceProvider.GetRequiredService<PaymentContext>();
        
        // Veritabanını otomatik güncelle
        await context.Database.MigrateAsync();
        
        Console.WriteLine("[Payment.API] Veritabanı başarıyla güncellendi.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Payment.API] Startup error: {ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"[Payment.API] Inner error: {ex.InnerException.Message}");
    }
}

app.Run();
