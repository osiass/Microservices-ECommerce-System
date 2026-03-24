using Common.DTOs;
namespace WebUI.Services
{
    public class CatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public CatalogService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient("GatewayClient");
            _config = config;
        }

        public string? GetFullImageUrl(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return null;
            if (imageUrl.StartsWith("http")) return imageUrl;
            
            var gatewayUrl = _config["GatewayExternalUrl"] ?? "https://localhost:7067"; 
            return $"{gatewayUrl.TrimEnd('/')}/catalog{imageUrl}";
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
        public async Task<bool> CreateProduct(string token, ProductDto product)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PostAsJsonAsync("/catalog/api/products", product);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProduct(string token, string id)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync($"/catalog/api/products/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> UploadImage(string token, Stream fileStream, string fileName)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileStream);
                content.Add(streamContent, "file", fileName);

                var response = await _httpClient.PostAsync("/catalog/api/images/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ImageUploadDto>();
                    return result?.Url;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Resim yükleme hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateProduct(string token, ProductDto product)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PutAsJsonAsync($"/catalog/api/products/{product.Id}", product);
            return response.IsSuccessStatusCode;
        }

        public async Task<RatingDto?> RateProduct(string id, int score)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"/catalog/api/products/{id}/rate", score);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RatingDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Puanlama hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<List<CommentDto>> GetComments(string productId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<CommentDto>>($"/catalog/api/products/{productId}/comments") ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yorum getirme hatası: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> AddComment(string productId, CommentDto comment)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"/catalog/api/products/{productId}/comments", comment);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Yorum ekleme hatası: {ex.Message}");
                return false;
            }
        }
    }
}
