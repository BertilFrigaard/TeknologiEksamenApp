using TeknologiEksamenApp.Services;
using TeknologiEksamenApp.Utils;

namespace TeknologiEksamenApp.Views.AfterLogin;

public partial class JoinGamePage : ContentPage
{
    private readonly GameService _gameService;
    public JoinGamePage(GameService gameService)
	{
        _gameService = gameService;
        InitializeComponent();
	}

    private async void BtnJoinClicked(object sender, EventArgs e)
    {
        string joinCode = EntryJoinCode.Text;
        string password = EntryPassword.Text;

        if (!InputVerifier.IsValidJoinCode(joinCode))
        {
            return;
        }

        GameService.GameIdResponse res = await _gameService.JoinGameAsync(joinCode, password);
        if (res.Success)
        {
            await Shell.Current.GoToAsync($"../{nameof(ViewGamePage)}?id={res.GameId}");
        }
        else
        {
            if (res.NeedsLogin)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Error", res.ErrorMessage, "OK");
            }
        }
    }

    private async void BtnReturnClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}