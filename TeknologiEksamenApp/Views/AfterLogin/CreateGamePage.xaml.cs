using TeknologiEksamenApp.Services;
using TeknologiEksamenApp.Utils;

namespace TeknologiEksamenApp.Views.AfterLogin;

public partial class CreateGamePage : ContentPage
{
    private readonly GameService _gameService;
	public CreateGamePage(GameService gameService)
	{
        _gameService = gameService;
		InitializeComponent();
	}
	
    private async void BtnCreateClicked(object sender, EventArgs e)
    {
        string? gameName = EntryName.Text;
        string? gamePassword = EntryPassword.Text;
        string? budget = EntryBudget.Text;
        string? period = EntryPeriod.Text;

        if (!InputVerifier.IsValidName(gameName))
        {
            return;
        }

        if (!InputVerifier.IsValidPassword(gamePassword) && gamePassword != null)
        {
            return;
        }

        if (!InputVerifier.IsValidAmount(budget))
        {
            return;
        }

        if (!InputVerifier.IsValidPeriod(period))
        {
            return;
        }

        int periodMinutes = (int)Math.Floor(float.Parse(period)) * 24 * 60;

        GameService.GameIdResponse res = await _gameService.CreateGameAsync(gameName, float.Parse(budget), periodMinutes, gamePassword);
        if (res.Success)
        {
            await Shell.Current.GoToAsync($"../{nameof(ViewGamePage)}?id={res.GameId}");
        }
        else
        {
            await DisplayAlert("ERROR", res.ErrorMessage, "OK");
        }
    }
    private async void BtnReturnClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}