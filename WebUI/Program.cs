using WebUI.Components;
using WebUI.Services;
using Blazored.LocalStorage;
using System.Globalization;

var trCulture = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = trCulture;
CultureInfo.DefaultThreadCurrentUICulture = trCulture;

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
builder.Services.AddScoped<DiscountService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<ToastService>();

// Authentication & Authorization 
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options => 
    {
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });
builder.Services.AddAuthorizationCore();
builder.Services.AddAntiforgery(options => 
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, WebUI.Providers.CustomAuthStateProvider>();
builder.Services.AddScoped<WebUI.Providers.CustomAuthStateProvider>(sp => (WebUI.Providers.CustomAuthStateProvider)sp.GetRequiredService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>());


// 401 geldiğinde otomatik login'e yönlendiren handler
builder.Services.AddTransient<AuthRedirectHandler>();

// API Gateway istemcisi - Aspire Service Discovery ile servis isminden adresi çözer
builder.Services.AddHttpClient("GatewayClient", client =>
{
    client.BaseAddress = new Uri("http://gateway");
}).AddHttpMessageHandler<AuthRedirectHandler>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
// app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
