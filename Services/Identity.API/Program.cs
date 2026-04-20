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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IdentityContext>();
    for (int attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("[Identity.API] Migration tamamlandı.");

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
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Identity.API] Migration denemesi {attempt}/10 başarısız: {ex.Message}");
            if (attempt == 10) Console.WriteLine("[Identity.API] Migration başarısız, devam ediliyor.");
            else await Task.Delay(3000);
        }
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
