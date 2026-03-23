using Common.DTOs;

namespace WebUI.Services
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;
        public OrderService(IHttpClientFactory httpClientFactory)
        {
            // aspire sayesinde gateway ismini kullanarak port derdinde kurtulduk
            _httpClient = httpClientFactory.CreateClient("GatewayClient");
        }

        public async Task<(bool Success, string Message)> Checkout(string token, BasketCheckoutDto checkoutData)
        {
            Console.WriteLine($"Checkout başlatıldı. User: {checkoutData.UserName}, Token mevcut: {!string.IsNullOrEmpty(token)}");
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"Token başlangıcı: {(token.Length > 10 ? token.Substring(0, 10) : token)}");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var response = await _httpClient.PostAsJsonAsync("/order/api/order/checkout", checkoutData, cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, string.Empty);
                }
                
                var error = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(error))
                {
                    error = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                }
                return (false, error);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ödeme/Checkout hatası: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<List<OrderDto>> GetOrders(string token, string userName)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var orders = await _httpClient.GetFromJsonAsync<List<OrderDto>>($"/order/api/order/{userName}");
                return orders ?? new List<OrderDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sipariş geçmişi hatası: {ex.Message}");
                return new List<OrderDto>();
            }
        }

        public async Task<OrderDto?> GetOrderById(string token, int id)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await _httpClient.GetFromJsonAsync<OrderDto>($"/order/api/order/detail/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sipariş detay hatası: {ex.Message}");
                return null;
            }
        }
        public async Task<string> GetAuthStatus(string token)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(token))
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                else
                    _httpClient.DefaultRequestHeaders.Authorization = null;

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                var response = await _httpClient.GetAsync("/order/api/order/test-auth", cts.Token);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Hata: {ex.Message}";
            }
        }
    }
}
