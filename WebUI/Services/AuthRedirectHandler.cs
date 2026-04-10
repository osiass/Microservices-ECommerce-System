using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace WebUI.Services;

public class AuthRedirectHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;

    public AuthRedirectHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var localStorage = scope.ServiceProvider.GetRequiredService<ILocalStorageService>();
                var nav = scope.ServiceProvider.GetRequiredService<NavigationManager>();

                await localStorage.RemoveItemAsync("authToken", cancellationToken);
                await localStorage.RemoveItemAsync("userName", cancellationToken);
                await localStorage.RemoveItemAsync("userRole", cancellationToken);

                nav.NavigateTo("/login", forceLoad: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthRedirectHandler] Redirect hatası: {ex.Message}");
            }
        }

        return response;
    }
}
