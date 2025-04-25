using System.Collections.ObjectModel;
using TeknologiEksamenApp.Services;
using TeknologiEksamenApp.Utils;

namespace TeknologiEksamenApp.Views.AfterLogin;

public partial class AddExpensePage : ContentPage
{
    private readonly GameService _gameService;
    private ObservableCollection<string?> options;
    private List<string?> gameIds;
	public AddExpensePage(UserService userService, GameService gameService)
	{
        _gameService = gameService;
		InitializeComponent();

        options = new();
        gameIds = new();
        options.Add("Alle Spil");
        gameIds.Add(null);

        UpdatePicker();

        LoadOptionsAsync(userService.ActiveUser.Games);
	}

    private void UpdatePicker()
    {
        PickerGame.ItemsSource = options;
        PickerGame.SelectedIndex = 0;
    }

    private async void LoadOptionsAsync(List<string> games)
    { 

        var tasks = games.Select(game => LoadOptionAsync(game)).ToList();
        await Task.WhenAll(tasks);

    }

    private async Task LoadOptionAsync(string gameId)
    {
        GameService.GameResponse response = await _gameService.GetGameAsync(gameId);
        if (response.Success)
        {
            options.Add(response.Game.Name);
            gameIds.Add(response.Game.Id);
        }
        else
        {
            if (response.NeedsLogin)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("ERROR", response.ErrorMessage, "OK");
            }
        }
    }

    private async Task AddExpenseAsync(string gameID, float amount)
    {
        if (gameID == null)
        {
            return;
        }
        GameService.GameIdResponse response = await _gameService.AddExpenseAsync(gameID, amount);
        if (!response.Success)
        {
            if (response.NeedsLogin)
            {
                await DisplayAlert("Session Expired", "You need to login again", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("ERROR", response.ErrorMessage, "OK");
            }
        }
    }

    private async void BtnAddClicked(object sender, EventArgs e)
    {
        int index = PickerGame.SelectedIndex;
        string amountString = EntryAmount.Text.Replace(",",".");
        
        if (!InputVerifier.IsValidAmount(amountString))
        {
            return;
        }
        float amount = float.Parse(amountString);
        if (index == 0)
        {
            var tasks = gameIds.Select(id => AddExpenseAsync(id, amount)).ToList();
            await Task.WhenAll(tasks);
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            string gameId = gameIds[index];
            await AddExpenseAsync(gameId, amount);
            await Shell.Current.GoToAsync("..");
        }
    }

    private void BtnReturnClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("..");
    }
}