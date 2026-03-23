using Basket.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Aspire defaults
builder.AddServiceDefaults();

// External Service Clients
builder.Services.AddHttpClient("Catalog", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Catalog"]!);
});

builder.Services.AddHttpClient("Discount", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Discount"]!);
});

// Database & API Configuration
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BasketContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication with Diagnostic Relaxations
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Teşhis için: Issuer doğrulamasını geçici olarak kapattık
            ValidateAudience = false, // Teşhis için: Audience doğrulamasını geçici olarak kapattık
            ValidateLifetime = false, // Teşhis için: Zaman uyuşmazlığı (clock skew) ihtimalini eledik
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
