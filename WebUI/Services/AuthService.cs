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

        public async Task<(string? Token, string? Role, string? Email)> Login(string username, string password)
        {
            var loginDto = new UserLoginDto { UserName = username, Password = password };
            var response = await _httpClient.PostAsJsonAsync("/identity/api/auth/login", loginDto);

            if(response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                return (result?.Token, result?.Role, result?.Email);
            }
            return (null, null, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> Register(UserRegisterDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync("/identity/api/auth/register", registerDto);
            string? error = null;
            if (!response.IsSuccessStatusCode)
            {
                error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AuthService] Register failed: {response.StatusCode} - {error}");
            }
            return (response.IsSuccessStatusCode, error);
        }

        public async Task<UserProfileDto?> GetProfile(string username)
        {
            return await _httpClient.GetFromJsonAsync<UserProfileDto>($"/identity/api/auth/profile/{username}");
        }

        public async Task<bool> UpdateProfile(string username, UpdateProfileDto updateDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"/identity/api/auth/profile/{username}", updateDto);
            return response.IsSuccessStatusCode;
        }
    }
}
