using Common.DTOs;
using Microsoft.AspNetCore.Components.Forms;

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
        public async Task<bool> CreateProduct(string token, ProductDto product, IBrowserFile? file = null)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(product.Name ?? ""), "Name");
                content.Add(new StringContent(product.Category ?? ""), "Category");
                content.Add(new StringContent(product.Description ?? ""), "Description");
                content.Add(new StringContent(product.Price.ToString()), "Price");
                content.Add(new StringContent(product.StockQuantity.ToString()), "StockQuantity");

                if (product.Features != null)
                {
                    foreach (var feature in product.Features)
                    {
                        content.Add(new StringContent(feature), "Features");
                    }
                }

                if (file != null)
                {
                    var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    content.Add(streamContent, "ImageFile", file.Name);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, "/catalog/api/products");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[CatalogService] CreateProduct Hatası: {response.StatusCode} - {CleanJsonError(errorBody) ?? errorBody}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CatalogService] CreateProduct Sistemsel Hata: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool Success, string Message)> DeleteProduct(string token, string id)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/catalog/api/products/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    var cleanError = CleanJsonError(errorBody) ?? $"Hata: {response.StatusCode}";
                    return (false, cleanError);
                }
                
                return (true, "Başarılı");
            }
            catch (Exception ex)
            {
                return (false, $"Sistem Hatası: {ex.Message}");
            }
        }

        public async Task<bool> UpdateProduct(string token, ProductDto product, IBrowserFile? file = null)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(product.Name ?? ""), "Name");
                content.Add(new StringContent(product.Category ?? ""), "Category");
                content.Add(new StringContent(product.Description ?? ""), "Description");
                content.Add(new StringContent(product.Price.ToString()), "Price");
                content.Add(new StringContent(product.StockQuantity.ToString()), "StockQuantity");

                if (product.Features != null)
                {
                    foreach (var feature in product.Features)
                    {
                        content.Add(new StringContent(feature), "Features");
                    }
                }

                if (file != null)
                {
                    var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    content.Add(streamContent, "ImageFile", file.Name);
                }

                var request = new HttpRequestMessage(HttpMethod.Put, $"/catalog/api/products/{product.Id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[CatalogService] UpdateProduct Hatası: {response.StatusCode} - {CleanJsonError(errorBody) ?? errorBody}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CatalogService] UpdateProduct Sistemsel Hata: {ex.Message}");
                return false;
            }
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
}
