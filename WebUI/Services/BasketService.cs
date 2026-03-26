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
                var content = await response.Content.ReadAsStringAsync();
                var cleanError = CleanJsonError(content) ?? $"Hata: {response.StatusCode}";
                Console.WriteLine($"[BasketService] Sepet silme hatası: {cleanError}");
                return (false, cleanError);
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
                var cleanError = CleanJsonError(content) ?? $"{response.ReasonPhrase}";
                return (false, cleanError);
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

    private string? CleanJsonError(string json)
    {
        try
        {
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(json);
            if (jsonDoc.RootElement.TryGetProperty("errors", out var errors))
            {
                var firstError = errors.EnumerateObject().FirstOrDefault();
                return firstError.Value.EnumerateArray().FirstOrDefault().GetString();
            }
            if (jsonDoc.RootElement.TryGetProperty("title", out var title)) return title.GetString();
            if (jsonDoc.RootElement.TryGetProperty("detail", out var detail)) return detail.GetString();
        }
        catch { }
        return null;
    }
}