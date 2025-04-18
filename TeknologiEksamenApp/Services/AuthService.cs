using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TeknologiEksamenApp.Services;

public class AuthService
{
    private const string BASE_URL = "http://10.0.2.2:3000";
    private const string LOGIN_ENDPOINT = "auth/login";
    private const string SIGNUP_ENDPOINT = "auth/register";
    private const string LOGOUT_ENDPOINT = "auth/logout";
    private const string REFRESH_SESSION_ENDPOINT = "auth/refreshSession";

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

    public async Task<HttpResponseMessage> DeleteAuthenticatedAsync(string? uri)
    {
        string? accessToken = await SecureStorage.Default.GetAsync("access_token");
        bool renewed = false;

        if (accessToken == null)
        {
            accessToken = await RenewAccessTokenAsync();
            renewed = true;
        }

        if (accessToken == null)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.DeleteAsync(uri);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !renewed)
        {
            accessToken = await RenewAccessTokenAsync();
            if (accessToken == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            response = await _httpClient.DeleteAsync(uri);
        }

        return response;
    }

    public async Task<HttpResponseMessage> PostAuthenticatedAsync(string? uri, StringContent? content = null)
    {
        string? accessToken = await SecureStorage.Default.GetAsync("access_token");
        bool renewed = false;

        if (accessToken == null)
        {
            accessToken = await RenewAccessTokenAsync();
            renewed = true;
        }

        if (accessToken == null)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.PostAsync(uri, content);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !renewed)
        {
            accessToken = await RenewAccessTokenAsync();
            if (accessToken == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            response = await _httpClient.PostAsync(uri, content);
        }

        return response;
    }

    public async Task<HttpResponseMessage> GetAuthenticatedAsync(string? uri)
    {
        string? accessToken = await SecureStorage.Default.GetAsync("access_token");
        bool renewed = false;

        if (accessToken == null)
        {
            accessToken = await RenewAccessTokenAsync();
            renewed = true;
        }

        if (accessToken == null)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(uri);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !renewed)
        {
            accessToken = await RenewAccessTokenAsync();
            if (accessToken == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            response = await _httpClient.GetAsync(uri);
        }

        return response;
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

    public async Task<bool> LogoutAsync()
    {
        //Add authentication
        HttpResponseMessage response = await PostAuthenticatedAsync($"{BASE_URL}/{LOGOUT_ENDPOINT}");
        
        ClearSession();
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<string?> RenewAccessTokenAsync()
    {
        var refreshToken = await SecureStorage.Default.GetAsync("refresh_token");
        var userId = await SecureStorage.Default.GetAsync("user_id");
        if (refreshToken == null || userId == null)
        {
            return null;
        }
        var requestData = new { refreshToken, userId };

        var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{BASE_URL}/{REFRESH_SESSION_ENDPOINT}", content);
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
            string? AccessToken = responseJson.GetProperty("accessToken").GetString();
            if (AccessToken == null)
            {
                return null;
            }
            await SecureStorage.Default.SetAsync("access_token", AccessToken);
            return AccessToken;
        } else
        {
            return null;
        }
    }

    public async Task<bool> HasSavedSession()
    {
        string? accessToken = await SecureStorage.Default.GetAsync("access_token");
        if (accessToken == null)
        {
            ClearSession();
            return false;
        }

        string? refreshToken = await SecureStorage.Default.GetAsync("refresh_token");
        if (refreshToken == null)
        {
            ClearSession();
            return false;
        }

        string? userId = await SecureStorage.Default.GetAsync("user_id");
        if (userId == null)
        {
            ClearSession();
            return false;
        }

        return true;

    }

    public async Task<string?> GetUserIdAsync()
    {
        return await SecureStorage.Default.GetAsync("user_id");
    }

    private void ClearSession()
    {
        SecureStorage.Default.RemoveAll();
    }
}
