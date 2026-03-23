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

        public async Task<List<ProductDto>> GetProducts(string? search = null, string? category = null)
        { 
            try
            {
                var url = "/catalog/api/products";
                var queryParams = new List<string>();
                
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={search}");
                if (!string.IsNullOrEmpty(category)) queryParams.Add($"category={category}");
                
                if (queryParams.Any())
                {
                    url += "?" + string.Join("&", queryParams);
                }

                return await _httpClient.GetFromJsonAsync<List<ProductDto>>(url) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Katalog ürün getirme hatası: {ex.Message}");
                return new List<ProductDto>();
            }
        }

        public async Task<ProductDto?> GetProductById(string id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ProductDto>($"/catalog/api/products/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün detay getirme hatası: {ex.Message}");
                return null;
            }
        }
    }
}
