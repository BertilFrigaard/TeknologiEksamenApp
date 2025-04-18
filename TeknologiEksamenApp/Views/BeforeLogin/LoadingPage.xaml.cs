using TeknologiEksamenApp.Services;
using TeknologiEksamenApp.Views.AfterLogin;

namespace TeknologiEksamenApp.Views.BeforeLogin;

public partial class LoadingPage : ContentPage
{
    private readonly AuthService _authService;
	public LoadingPage(AuthService authService)
	{
        _authService = authService;
        InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CheckSession();
    }

    private async void CheckSession()
    {
        if (await _authService.HasSavedSession())
        {
            await Shell.Current.GoToAsync(nameof(AccountPage));
        }
        else
        {
            await Shell.Current.GoToAsync(nameof(OnboardingPage));
        }
    }
}