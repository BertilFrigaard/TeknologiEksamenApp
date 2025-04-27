using System.Collections.ObjectModel;
using TeknologiEksamenApp.Services;

namespace TeknologiEksamenApp.Views.AfterLogin;

public partial class AccountPage : ContentPage
{
	private readonly AuthService _authService;
	private readonly UserService _userService;
	private readonly GameService _gameService;
	public AccountPage(AuthService authService, UserService userService, GameService gameService)
	{
		_userService = userService;
		_authService = authService;
		_gameService = gameService;
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		LoadUserData();
    }
	
    private void BtnRefreshClicked(object sender, EventArgs e)
    {
		LoadUserData();
    }

    private void BtnLogoutClicked(object sender, EventArgs e)
    {
		Logout();
    }

	private async void BtnAddExpenseClicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync(nameof(AddExpensePage));
    }

    private async void BtnCreateGameClicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync(nameof(CreateGamePage));
    }

    private async void BtnJoinGameClicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync(nameof(JoinGamePage));
    }


	private async void Logout()
	{
		await _authService.LogoutAsync();
		_userService.ClearCurrentUser();
		await Shell.Current.GoToAsync("..");
	}

	private async void LoadUserData()
	{
		UserService.UserResponse response = await _userService.UpdateCurrentUserAsync();
		if (!response.Success)
		{
			if (response.NeedsLogin)
			{
				await DisplayAlert("Error", response.ErrorMessage, "OK");
				Logout();
				return;
			}
            else
            {
				await DisplayAlert("Error", response.ErrorMessage, "OK");
				return;
            }
        }

		if (response.User == null)
		{
			return;
		}

		LabelUsername.Text = response.User.Username;

		ObservableCollection<GameCard> gameCards = new ObservableCollection<GameCard>();

		await LoadAllGamesAsync(response.User.Games, gameCards);

		if (gameCards.Count <= 0)
		{
			GameCard card = new GameCard
			{
				Title = "No active games"
			};
			gameCards.Add(card);
		}

		CollectionGameCards.ItemsSource = gameCards;
	}
	        
	private async Task LoadAllGamesAsync(List<string> games, ObservableCollection<GameCard> gameCards)
	{
		var tasks = games.Select(game => LoadGameCard(game, gameCards)).ToList();
		await Task.WhenAll(tasks);
	}

	private async Task LoadGameCard(string gameId, ObservableCollection<GameCard> gameCards)
	{
		GameService.GameResponse gameResponse = await _gameService.GetGameAsync(gameId);
		if (!gameResponse.Success)
		{
			if (gameResponse.NeedsLogin)
			{
				Logout();
				return;
			}
			else
			{
				GameCard card = new GameCard
				{
					Title = "ERROR"
				};
				gameCards.Add(card);
			}
		}
		else
		{
			GameCard card = new GameCard
			{
				Title = gameResponse.Game.Name,
				ID = gameResponse.Game.Id
			};
			gameCards.Add(card);
		}
	}

	public class GameCard
	{
		public string Title { get; set; } = "Game Title";
		public string? ID { get; set; }
	}

    private void CollectionGameCardsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
		var selection = e.CurrentSelection;
		CollectionGameCards.SelectedItem = null; 
		if (selection.Count < 1)
		{
			return;
		}
		GameCard selectedCard = (GameCard) selection[0];
		if (String.IsNullOrEmpty(selectedCard.ID))
		{
			return;
		}
		Shell.Current.GoToAsync($"{nameof(ViewGamePage)}?id=" + selectedCard.ID);
    }
}