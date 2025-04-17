using System.Text;
using System.Text.Json;

namespace TeknologiEksamenApp.Services;

public class AuthService
{
    private const string BASE_URL = "http://10.0.2.2:3000";
    private const string LOGIN_ENDPOINT = "auth/login";
    private const string SIGNUP_ENDPOINT = "auth/register";

    private readonly HttpClient _httpClient;
    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public bool NeedsVerifaction { get; set; } = false;
    }

    public async Task<AuthResult> SignupAsync(string email, string password, string username)
    {
        var signupData = new { email, password, username };
        var content = new StringContent(JsonSerializer.Serialize(signupData), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{BASE_URL}/{SIGNUP_ENDPOINT}", content);

        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.Created:
                return new AuthResult { Success = true };

            case System.Net.HttpStatusCode.Conflict:
                return new AuthResult { Success = false, ErrorMessage = "Email already in use." };

            default:
                return new AuthResult { Success = false, ErrorMessage = "An error occurred during signup." };
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var loginData = new { email, password };
        var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{BASE_URL}/{LOGIN_ENDPOINT}", content);

        switch (response.StatusCode)
        {

            case System.Net.HttpStatusCode.OK:
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

                string? AccessToken = responseJson.GetProperty("accessToken").GetString();
                string? RefreshToken = responseJson.GetProperty("refreshToken").GetString();
                string? UserId = responseJson.GetProperty("userId").GetString();

                if (AccessToken == null || RefreshToken == null || UserId == null)
                {
                    return new AuthResult { Success = false, ErrorMessage = "Something went wrong. Try again later!" };
                }
                await SecureStorage.SetAsync("access_token", AccessToken);
                await SecureStorage.SetAsync("refresh_token", RefreshToken);
                await SecureStorage.SetAsync("user_id", UserId);
                return new AuthResult { Success = true };

            case System.Net.HttpStatusCode.Forbidden:
                return new AuthResult { Success = false, ErrorMessage = "User not verified. Check your email.", NeedsVerifaction = true };
                
            case System.Net.HttpStatusCode.Unauthorized:
                return new AuthResult { Success = false, ErrorMessage = "Invalid email or password!" };

            default:
                return new AuthResult { Success = false, ErrorMessage = "An error occurred during login." };
        }
    }
}
