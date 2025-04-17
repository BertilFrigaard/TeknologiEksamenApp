using TeknologiEksamenApp.Services;
using TeknologiEksamenApp.Utils;
using TeknologiEksamenApp.Views.AfterLogin;

namespace TeknologiEksamenApp.Views.BeforeLogin;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    public LoginPage(AuthService authService)
	{
        _authService = authService;
        InitializeComponent();
	}
	
    private async void BtnLoginClicked(object sender, EventArgs e)
    {
        string? email = EntryEmail.Text;
        string? password = EntryPassword.Text;

        if (!InputVerifier.IsValidEmail(email))
        {
            return;
        }

        if (!InputVerifier.IsValidPassword(password))
        {
            return;
        }

        AuthService.AuthResult result = await _authService.LoginAsync(email, password);

        if (result.Success)
        {
            await Shell.Current.GoToAsync($"../../{nameof(AccountPage)}");
        }
        else
        {
            await DisplayAlert("Error", result.ErrorMessage, "OK");
            if (result.NeedsVerifaction)
            {
                await Shell.Current.GoToAsync($"../{nameof(MissingVerifactionPage)}");
            }
        }
    }
	
    private async void BtnReturnClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}