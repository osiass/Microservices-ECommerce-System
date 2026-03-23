using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Order.API.Data;
using System.Text;
using System;
using Common.EventBus;
using RabbitMQ.Client;

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
            ValidateIssuer = false, // Teşhis için: Geçici olarak kapattık
            ValidateAudience = false, // Teşhis için: Geçici olarak kapattık
            ValidateLifetime = false, // Teşhis için: Geçici olarak kapattık
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

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
