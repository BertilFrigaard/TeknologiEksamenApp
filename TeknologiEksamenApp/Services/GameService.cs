using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeknologiEksamenApp.Services;

public class GameService
{
    private const string BASE_URL = "http://10.0.2.2:3000";
    private const string CREATE_GAME_ENDPOINT = "games/createGame";
    private const string GET_GAME_ENDPOINT = "games/getGameById";
    private const string JOIN_GAME_ENDPOINT = "games/joinGame";
    private const string ADD_EXPENSE_ENDPOINT = "games/addEntry";

    private readonly AuthService _authService;
    public GameService(AuthService authService)
    {
        _authService = authService;
    }

    public class GameIdResponse
    {
        public required bool Success { get; set; }
        public bool NeedsLogin { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public string? GameId {  get; set; }
    }

    public class GameResponse
    {
        public required bool Success { get; set; }
        public bool NeedsLogin { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public Game? Game {  get; set; }
    }

    public class Game
    {
        [JsonPropertyName("_id")]
        public required string Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("players")]
        public required List<GamePlayer> GamePlayers { get; set; }

        [JsonPropertyName("admin")]
        public required string AdminId { get; set; }

        [JsonPropertyName("joinCode")]
        public required string JoinCode { get; set; }

        [JsonPropertyName("createdAt")]
        public required DateTime Created { get; set; }

        [JsonPropertyName("period")]
        public required int PeriodMinutes { get; set; }

        [JsonPropertyName("budget")]
        public required float Budget {  get; set; }
    }

    public class GamePlayer
    {
        [JsonPropertyName("id")]
        public required string PlayerId { get; set; }

        [JsonPropertyName("entries")]
        public required List<GameEntry> Entries { get; set; }
    }

    public class GameEntry
    {
        [JsonPropertyName("date")]
        public required DateTime Created { get; set; }

        [JsonPropertyName("amount")]
        public required float Amount { get; set; }
    }

    public async Task<GameIdResponse> CreateGameAsync(string gameName, float budget, string? password = null)
    {
        var requestData = new { gameName, budget, password };
        var requestContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
        
        HttpResponseMessage response = await _authService.PostAuthenticatedAsync($"{BASE_URL}/{CREATE_GAME_ENDPOINT}", requestContent);

        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.Unauthorized:
                return new GameIdResponse { Success = false, NeedsLogin = true, ErrorMessage = "Session expired" };

            case System.Net.HttpStatusCode.Created:
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

                string? gameId = responseJson.GetProperty("gameId").GetString()!;
                return new GameIdResponse { Success = true, GameId = gameId };

            case System.Net.HttpStatusCode.RequestEntityTooLarge:
                return new GameIdResponse { Success = false, ErrorMessage = "Input too long" };

            default:
                return new GameIdResponse { Success = false, ErrorMessage = "Something went wrong! Try again later." };
        }
    }

    public async Task<GameResponse> GetGameAsync(string gameId)
    {
        HttpResponseMessage response = await _authService.GetAuthenticatedAsync($"{BASE_URL}/{GET_GAME_ENDPOINT}/{gameId}");
        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.Unauthorized:
                return new GameResponse { Success = false, NeedsLogin = true, ErrorMessage = "Session expired" };

            case System.Net.HttpStatusCode.NotFound:
                return new GameResponse { Success = false, ErrorMessage = "Game not found" };

            case System.Net.HttpStatusCode.Forbidden:
                return new GameResponse { Success = false, ErrorMessage = "Missing permission" };

            case System.Net.HttpStatusCode.Found:
                var responseContent = await response.Content.ReadAsStringAsync();

                Game? game = JsonSerializer.Deserialize<Game>(responseContent);
                if (game == null)
                {
                    return new GameResponse { Success = false, ErrorMessage = "Error while decoding game object" };
                }

                return new GameResponse { Success = true, Game = game };

            default:
                return new GameResponse { Success = false, ErrorMessage = "Something went wrong! Try again later." };

        }
    }

    public async Task<GameIdResponse> JoinGameAsync(string joinCode, string? password = null)
    {
        var requestData = new { joinCode, password };
        var requestContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _authService.PostAuthenticatedAsync($"{BASE_URL}/{JOIN_GAME_ENDPOINT}", requestContent);
        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.Unauthorized:
                return new GameIdResponse { Success = false, NeedsLogin = true, ErrorMessage = "Session expired" };
            case System.Net.HttpStatusCode.OK:
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                string? gameId = responseJson.GetProperty("gameId").GetString()!;
                return new GameIdResponse { Success = true, GameId = gameId };
            case System.Net.HttpStatusCode.Forbidden:
                return new GameIdResponse { Success = false, ErrorMessage = (password == null ? "Game is password locked" : "Incorrect password") };
            case System.Net.HttpStatusCode.NotFound:
                return new GameIdResponse { Success = false, ErrorMessage = "Game not found" };
            default:
                return new GameIdResponse { Success = false, ErrorMessage = "Something went wrong! Try again later." };
        }
    }

    public async Task<GameIdResponse> AddExpenseAsync(string gameId, float amount)
    {
        var requestData = new { gameId, amount };
        var requestContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _authService.PostAuthenticatedAsync($"{BASE_URL}/{ADD_EXPENSE_ENDPOINT}", requestContent);
        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.Unauthorized:
                return new GameIdResponse { Success = false, NeedsLogin = true, ErrorMessage = "Session expired" };

            case System.Net.HttpStatusCode.OK:
                return new GameIdResponse { Success = true, GameId = gameId };

            case System.Net.HttpStatusCode.Forbidden:
                return new GameIdResponse { Success = false, ErrorMessage = "You are not a part of this game" };

            case System.Net.HttpStatusCode.RequestEntityTooLarge:
                return new GameIdResponse { Success = false, ErrorMessage = "Amount too large" };

            default:
                return new GameIdResponse { Success = false, ErrorMessage = "Something went wrong! Try again later." };
        }
    }
}

