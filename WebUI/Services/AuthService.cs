using Common.DTOs;

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
            var loginDto = new UserLoginDto { UserName = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync("/identity/api/auth/login", loginDto);

            if(response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                return result?.Token;
            }
            return null;
        }

        public async Task<bool> Register(UserRegisterDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync("/identity/api/auth/register", registerDto);
            return response.IsSuccessStatusCode;
        }
    }
    public class LoginResponse { public string Token { get; set; } = ""; }
}
