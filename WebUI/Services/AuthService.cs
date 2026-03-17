namespace WebUI.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GatewayClient");
        }

        public async Task<string?> Login(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/identity/api/auth/login", new { username, password });

            if(response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                return result?.Token;
            }
            return null;
        }
    }
    public class LoginResponse { public string Token { get; set; } = ""; }
}
