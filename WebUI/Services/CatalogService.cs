using Common.DTOs;
namespace WebUI.Services
{
    public class CatalogService
    {
        private readonly HttpClient _httpClient;
        public CatalogService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GatewayClient");
        }

        public async Task<List<ProductDto>> GetProducts()
        { //gateway üzerinden productsa git
            return await _httpClient.GetFromJsonAsync<List<ProductDto>>("/catalog/api/products") ?? new();
        }
    }
}
