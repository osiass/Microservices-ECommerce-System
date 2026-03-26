using Common.DTOs;

namespace WebUI.Services
{
    public class InventoryService
    {
        private readonly HttpClient _httpClient;

        public InventoryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GatewayClient");
        }

        public async Task<List<StockDto>> GetStocks(string? token = null)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                return await _httpClient.GetFromJsonAsync<List<StockDto>>("/inventory/api/Stock", cts.Token) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryService] GetStocks hatası: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> SeedStocks(string? token, List<StockDto> stocks)
        {
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.PostAsJsonAsync("/inventory/api/Stock/seed", stocks);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[InventoryService] SeedStocks Hatası: {response.StatusCode} - {error}");
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryService] Stok tohumlama hatası: {ex.Message}");
                return false;
            }
        }
    }
}
