
using System.Collections.ObjectModel;
using TeknologiEksamenApp.Services;

namespace TeknologiEksamenApp.Views.AfterLogin;

public partial class ViewGamePage : ContentPage, IQueryAttributable
{
    private GameService.Game? game = null;
    private readonly GameService _gameService;
    private readonly UserService _userSerivce;
	public ViewGamePage(GameService gameService, UserService userService)
	{
        _gameService = gameService;
        _userSerivce = userService;
		InitializeComponent();
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var id))
        {
            LoadGame((string) id);
        }
        else 
        {
            DisplayAlert("Error", "Something went wrong!", "OK");
            Shell.Current.GoToAsync("..");
        }
    }

    private async void LoadGame(string id)
    {
        GameService.GameResponse response = await _gameService.GetGameAsync(id);
        if (response.Success && response.Game != null)
        {
            game = response.Game;
            LabelGameName.Text = game.Name;
            LabelJoinCode.Text = $"Join Code: {game.JoinCode}";

            ObservableCollection<PlayerCard> cards = new();
            await LoadAllPlayersAsync(game.GamePlayers, cards);

            CollectionPlayerCards.ItemsSource = cards;
     
        }
        else
        {
            await DisplayAlert("Error", response.ErrorMessage, "OK");
        }
    }

    private async Task LoadAllPlayersAsync(List<GameService.GamePlayer> players, ObservableCollection<PlayerCard> cards)
    {
        var tasks = players.Select(player => LoadPlayerAsync(player, cards)).ToList();
        await Task.WhenAll(tasks);
    }

    private async Task LoadPlayerAsync(GameService.GamePlayer player, ObservableCollection<PlayerCard> cards)
    {
        UserService.UserResponse response = await _userSerivce.GetUserAsync(player.PlayerId);
        if (response.Success)
        {
            cards.Add(new PlayerCard
                {
                    Name = response.User.Username,
                    ID = response.User.ID
                });
        }
        else
        {
            if (response.NeedsLogin)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                cards.Add(new PlayerCard
                    {
                        Name = "ERROR"
                    });
            }
        }
    }

    class PlayerCard
    {
        public required string Name { get; set; }
        public string? ID { get; set; }
        public float MoneyLeft { get; set; } = 0;
    };

    private async void BtnReturnClicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync("..");
    }

    private void CollectionPlayerCardsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
}