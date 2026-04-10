using Common.DTOs;
using System.Net.Http.Headers;

namespace WebUI.Services
{
    public class DiscountService
    {
        private readonly HttpClient _httpClient;
        public DiscountService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GatewayClient");
        }

        private void SetAuth(string? token)
        {
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            else
                _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<CouponDto?> GetDiscount(string code, string? token = null)
        {
            try
            {
                SetAuth(token);
                var response = await _httpClient.GetAsync($"/discount/api/discount/{code}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CouponDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<CouponDto>> GetCoupons(string? token = null)
        {
            try
            {
                SetAuth(token);
                return await _httpClient.GetFromJsonAsync<List<CouponDto>>("/discount/api/discount") ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task<(bool Success, string Message)> CreateDiscount(CouponDto coupon, string? token = null)
        {
            try
            {
                SetAuth(token);
                var response = await _httpClient.PostAsJsonAsync("/discount/api/discount", coupon);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DiscountService] CreateDiscount HATA: {response.StatusCode} - {content}");
                    return (false, $"{response.StatusCode}: {content}");
                }
                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<bool> UpdateDiscount(int id, CouponDto coupon, string? token = null)
        {
            try
            {
                SetAuth(token);
                var response = await _httpClient.PutAsJsonAsync($"/discount/api/discount/{id}", coupon);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteDiscount(int id, string? token = null)
        {
            try
            {
                SetAuth(token);
                var response = await _httpClient.DeleteAsync($"/discount/api/discount/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
