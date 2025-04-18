using System.Text.Json;

namespace TeknologiEksamenApp.Services;

public class UserService
{
    private const string BASE_URL = "http://10.0.2.2:3000";
    private const string GET_USER_ENDPOINT = "users/getUserById";

    private readonly AuthService _authService;

    private User? ActiveUser = null;

    public class UserResponse
    {
        public required bool Success { get; set; }
        public bool NeedsLogin { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public User? User { get; set; }
    }

    public class User
    {
        public required string Username { get; set; }
        public required string ID { get; set; }
        public List<string> Games { get; set; } = new List<string>();

    }
    public UserService(AuthService authService)
    {
        _authService = authService;
    }

    public async Task<UserResponse> UpdateCurrentUserAsync()
    {
        string? userId = null;
        if (ActiveUser != null)
        {
            userId = ActiveUser.ID;
        }

        if (userId == null)
        {
            userId= await _authService.GetUserIdAsync();
        }

        if (userId == null)
        {
            return new UserResponse { Success = false, NeedsLogin = true };
        }

        UserResponse response = await GetUserAsync(userId);
        if (response.Success)
        {
            ActiveUser = response.User;
        }
        return response;
    }

    public async Task<UserResponse> GetUserAsync(string userId)
    {
        HttpResponseMessage response = await _authService.GetAuthenticatedAsync($"{BASE_URL}/{GET_USER_ENDPOINT}/{userId}");

        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.Unauthorized:
                return new UserResponse { Success = false, NeedsLogin = true, ErrorMessage = "Session expired" };

            case System.Net.HttpStatusCode.NotFound:
                return new UserResponse { Success = false, ErrorMessage = "User not found" };

            case System.Net.HttpStatusCode.OK:
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                var user = new User
                {
                    Username = responseJson.GetProperty("username").GetString()!,
                    ID = responseJson.GetProperty("_id").GetString()!
                };

                if (responseJson.TryGetProperty("games", out var gamesElement))
                {
                    user.Games = gamesElement.EnumerateArray()
                                             .Select(x => x.ToString())
                                             .ToList();
                }

                return new UserResponse { Success = true, User = user };

            default:
                return new UserResponse { Success = false, ErrorMessage = "Something went wrong! Try again later." };
        }
    }

    public void ClearCurrentUser()
    {
        ActiveUser = null;
    }
}
