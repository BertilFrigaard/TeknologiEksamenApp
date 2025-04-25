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

        GameService.GameIdResponse res = await _gameService.CreateGameAsync(gameName, float.Parse(budget), gamePassword);
        await Shell.Current.GoToAsync($"../{nameof(ViewGamePage)}?id={res.GameId}");
    }
    private async void BtnReturnClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}