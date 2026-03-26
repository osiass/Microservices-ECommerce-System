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

        public async Task<List<StockDto>> GetStocks()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<StockDto>>("/inventory/api/stock") ?? new();
            }
            catch
            {
                return new();
            }
        }

        
    }
}
