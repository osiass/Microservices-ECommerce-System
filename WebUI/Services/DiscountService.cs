using Common.DTOs;

namespace WebUI.Services
{
    public class DiscountService
    {
        private readonly HttpClient _httpClient;
        public DiscountService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GatewayClient");
        }

        public async Task<CouponDto?> GetDiscount(string code)
        {
            try
            {
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

        public async Task<List<CouponDto>> GetCoupons()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<CouponDto>>("/discount/api/discount") ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task<bool> CreateDiscount(CouponDto coupon)
        {
            var response = await _httpClient.PostAsJsonAsync("/discount/api/discount", coupon);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateDiscount(int id, CouponDto coupon)
        {
            var response = await _httpClient.PutAsJsonAsync($"/discount/api/discount/{id}", coupon);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDiscount(int id)
        {
            var response = await _httpClient.DeleteAsync($"/discount/api/discount/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
