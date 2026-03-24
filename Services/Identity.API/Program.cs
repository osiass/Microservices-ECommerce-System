using Identity.API.Data;
using Identity.API.Services;
using Identity.API.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<TokenService>();
var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Veritabanı Otomatik Güncelleme ve Admin Tanımlama
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<IdentityContext>();
        // Bekleyen migration varsa uygula 
        await context.Database.MigrateAsync();

        var ahmetUser = await context.AppUsers.FirstOrDefaultAsync(u => u.UserName == "ahmetkoc");
        if (ahmetUser == null)
        {
            ahmetUser = new AppUser { UserName = "ahmetkoc", Email = "ahmetkoc@gmail.com", Role = "Admin" };
            context.AppUsers.Add(ahmetUser);
        }
        ahmetUser.Role = "Admin";
        ahmetUser.Password = BCrypt.Net.BCrypt.HashPassword("ahmet123");
        Console.WriteLine("[Identity.API] 'ahmetkoc' şifresi 'ahmet123' olarak güncellendi.");

        // Admin Kullanıcısı Seed/Update
        var adminUser = await context.AppUsers.FirstOrDefaultAsync(u => u.UserName == "admin");
        if (adminUser == null)
        {
            adminUser = new AppUser { UserName = "admin", Email = "admin@ecommerce.com", Role = "Admin" };
            context.AppUsers.Add(adminUser);
        }
        adminUser.Role = "Admin";
        adminUser.Password = BCrypt.Net.BCrypt.HashPassword("adminpassword");
        Console.WriteLine("[Identity.API] 'admin' şifresi 'adminpassword' olarak güncellendi.");

        await context.SaveChangesAsync();
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
