
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

            var dateNow = DateTime.Now;
            var dateEnd = game.Created.AddMinutes(game.PeriodMinutes);

            var timeDiff = dateEnd - dateNow;

            if (timeDiff.TotalDays >= 1)
            {
                LabelTimeLeft.Text = $"{(int) timeDiff.TotalDays} days left";
            }
            else if (timeDiff.TotalHours >= 1)
            {
                LabelTimeLeft.Text = $"{(int) timeDiff.TotalHours} hours left";
            }
            else
            {
                LabelTimeLeft.Text = $"{(int) timeDiff.TotalMinutes} minutes left";
            }

            LabelBudget.Text = $"Budget: {game.Budget} DKK";
            LabelJoinCode.Text = $"Join Code: {game.JoinCode}";

            if (game.AdminId.Equals(_userSerivce.ActiveUser?.ID))
            {
                BtnQuit.Text = "DELETE GAME";
            }
            else
            { 
                BtnQuit.Text = "LEAVE GAME";
            }

                ObservableCollection<PlayerCard> cards = new();
            await LoadAllPlayerCards(game, cards);

            CollectionPlayerCards.ItemsSource = cards;
     
        }
        else
        {
            await DisplayAlert("Error", response.ErrorMessage, "OK");
        }
    }

    private async Task LoadAllPlayerCards(GameService.Game game, ObservableCollection<PlayerCard> cards)
    {
        Dictionary<string, float> totalExpenses = game.GetTotalExpenses();
        var tasks = game.GamePlayers.Select(player => LoadPlayerAsync(player, totalExpenses[player.PlayerId], game.Budget, cards)).ToList();
        await Task.WhenAll(tasks);
    }

    private async Task LoadPlayerAsync(GameService.GamePlayer player, float expenseTotal, float budget, ObservableCollection<PlayerCard> cards)
    {
        UserService.UserResponse response = await _userSerivce.GetUserAsync(player.PlayerId);
        if (response.Success)
        {
            cards.Add(new PlayerCard
                {
                    Name = response.User.Username,
                    ID = response.User.ID,
                    MoneyLeft = budget - expenseTotal
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

    private async void CollectionPlayerCardsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var user = _userSerivce.ActiveUser;
        if (game.AdminId.Equals(user.ID))
        {
            var answer = await DisplayActionSheet($"KICK {((PlayerCard)CollectionPlayerCards.SelectedItem).Name}", "NO", "YES");
            if (answer != "YES")
            {
                return;
            }
            GameService.GameIdResponse res = await _gameService.KickFromGameAsync(((PlayerCard)CollectionPlayerCards.SelectedItem).ID, game.Id);
            if (res.Success)
            {
                await DisplayAlert("KICKED", $"You kicked {((PlayerCard)CollectionPlayerCards.SelectedItem).Name}", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("ERROR", res.ErrorMessage, "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
    }

    private async void BtnQuitClicked(object sender, EventArgs e)
    {
        GameService.GameIdResponse res;
        if (game.AdminId.Equals(_userSerivce.ActiveUser?.ID))
        {
            res = await _gameService.DeleteGameAsync(game.Id);
        }
        else
        { 
            res = await _gameService.LeaveGameAsync(game.Id);
        }
        if (res.Success)
        {
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlert("ERROR", res.ErrorMessage, "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}