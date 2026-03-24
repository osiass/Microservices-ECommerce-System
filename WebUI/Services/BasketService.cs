using Common.DTOs;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace WebUI.Services;

public class BasketService
{
    private readonly HttpClient _httpClient;

    public BasketService(IHttpClientFactory httpClientFactory)
    {
        // Gateway üzerinden gidecek olan aracımızı alıyoruz
        _httpClient = httpClientFactory.CreateClient("GatewayClient");
    }

    //Kullanıcının sepetini getiren metod 
    public async Task<ShoppingCartDto> GetBasket(string token, string userName)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            var response = await _httpClient.GetFromJsonAsync<ShoppingCartDto>($"/basket/api/basket?userName={userName.ToLower()}&t={DateTime.Now.Ticks}");
            return response ?? new ShoppingCartDto { UserName = userName };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sepet yükleme hatası: {ex.Message}");
            return new ShoppingCartDto { UserName = userName };
        }
    }

    //Sipariş tamamlanınca sepeti silmek için 
    public async Task<(bool Success, string Message)> DeleteBasket(string token, string userName)
    {
        var cleanedUser = (userName ?? "").Trim().ToLower();
        try
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            
            // PATH üzerinden siliyoruz artık
            var response = await _httpClient.DeleteAsync($"/basket/api/basket/{cleanedUser}");
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[BasketService] Sepet silme hatası ({response.StatusCode}): {error}");
                
                // İkinci bir şans: query string ile de dene 
                var retryResponse = await _httpClient.DeleteAsync($"/basket/api/basket?userName={cleanedUser}");
                if (retryResponse.IsSuccessStatusCode) return (true, "Retry worked");
                
                return (false, $"API Error: {response.StatusCode} - {error}");
            }
            return (true, "Success");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BasketService] Sepet silme istisnası: {ex.Message}");
            return (false, $"Exception: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> AddItem(string token, string userName, ProductDto product)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            var request = new AddItemRequestDto
            {
                UserName = userName,
                ProductId = product.Id.ToString(),
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1,
                ImageUrl = product.ImageUrl
            };

            var response = await _httpClient.PostAsJsonAsync("/basket/api/basket/add-item", request);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return (false, $"{(int)response.StatusCode} {response.ReasonPhrase}: {content}");
            }
            return (true, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sepete ürün ekleme hatası: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task UpdateBasket(string token, ShoppingCartDto cart)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            await _httpClient.PostAsJsonAsync("/basket/api/basket", cart);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sepet güncelleme hatası: {ex.Message}");
        }
    }
}