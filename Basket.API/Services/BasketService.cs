using Common.Dtos;
using Common.DTOs;
using System.Net.Http.Json;

namespace WebUI.Services;

public class BasketService
{
    private readonly HttpClient _httpClient;

    public BasketService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("GatewayClient");
    }

    // APIdan sepeti getiren metod 
    public async Task<ShoppingCartDto> GetBasket(string token, string userName)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Gateway üzerinden Basket APIya git
        var response = await _httpClient.GetFromJsonAsync<ShoppingCartDto>($"/basket/api/basket?userName={userName}");
        return response ?? new ShoppingCartDto { UserName = userName };
    }

    //Sepete yeni ürün ekleyen asıl metod
    public async Task AddToBasket(string token, string userName, ProductDto product)
    {
        // Önce mevcut sepeti yukarıdaki GetBasket metoduyla alıyoruz
        var currentBasket = await GetBasket(token, userName);

        // Sepete yeni ürünü ekliyoruz
        currentBasket.Items.Add(new BasketItemDto
        {
            ProductId = product.Id.ToString(),
            ProductName = product.Name,
            Price = product.Price,
            Quantity = 1
        });

        // Güncellenmiş sepeti APIya UpdateBasket controllerına POST
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        await _httpClient.PostAsJsonAsync("/basket/api/basket", currentBasket);
    }
}