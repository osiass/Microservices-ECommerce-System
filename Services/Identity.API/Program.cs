using Identity.API.Data;
using Identity.API.Services;
using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using Common.Middleware;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<TokenService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Veritabanı Otomatik Güncelleme ve Admin Tanımlama
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<IdentityContext>();
        await context.Database.MigrateAsync();

        // Admin seed - yoksa otomatik oluştur
        if (!await context.AppUsers.AnyAsync(u => u.Role == "Admin"))
        {
            context.AppUsers.Add(new Identity.API.Entities.AppUser
            {
                UserName = "admin",
                Email = "admin@ecommerce.com",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin"
            });
            await context.SaveChangesAsync();
            Console.WriteLine("[Identity.API] Admin kullanıcısı oluşturuldu.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Identity.API] Veritabanı güncelleme/seed hatası: {ex.Message}");
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
