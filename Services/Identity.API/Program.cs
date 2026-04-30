using Identity.API.Data;
using Identity.API.Services;
using Identity.API.Entities;
using Microsoft.EntityFrameworkCore;
using Common.Middleware;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

if (args.Contains("--migrate"))
{
    builder.Services.AddDbContext<IdentityContext>(o =>
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    var migrateApp = builder.Build();
    using var scope = migrateApp.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<IdentityContext>();
    for (int i = 1; i <= 20; i++)
    {
        try
        {
            await db.Database.MigrateAsync();
            if (!db.AppUsers.Any())
            {
                db.AppUsers.Add(new AppUser { UserName = "admin", Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Email = "admin@ecommerce.com", Role = "Admin" });
                await db.SaveChangesAsync();
            }
            Console.WriteLine("[Identity.API] Migration OK.");
            return;
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            var pending = await db.Database.GetPendingMigrationsAsync();
            foreach (var m in pending)
                await db.Database.ExecuteSqlRawAsync($"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{m}', '10.0.5') ON CONFLICT DO NOTHING");
            if (!db.AppUsers.Any())
            {
                db.AppUsers.Add(new AppUser { UserName = "admin", Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Email = "admin@ecommerce.com", Role = "Admin" });
                await db.SaveChangesAsync();
            }
            Console.WriteLine("[Identity.API] Migration history güncellendi.");
            return;
        }
        catch (Exception ex) { Console.WriteLine($"[Identity.API] Migration {i}/20: {ex.Message}"); if (i == 20) { Environment.Exit(1); } await Task.Delay(3000); }
    }
    return;
}

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
