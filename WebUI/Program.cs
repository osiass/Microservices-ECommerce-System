using WebUI.Components;
using WebUI.Services;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<BasketService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ToastService>();

// Authentication & Authorization 
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies");
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, WebUI.Providers.CustomAuthStateProvider>();
builder.Services.AddScoped<WebUI.Providers.CustomAuthStateProvider>(sp => (WebUI.Providers.CustomAuthStateProvider)sp.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>());


// API Gateway istemcisi - Aspire Service Discovery ile servis isminden adresi çözer
builder.Services.AddHttpClient("GatewayClient", client =>
{
    client.BaseAddress = new Uri("https://gateway");
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
