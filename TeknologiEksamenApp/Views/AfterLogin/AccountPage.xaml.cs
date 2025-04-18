using TeknologiEksamenApp.Services;

namespace TeknologiEksamenApp.Views.AfterLogin;

public partial class AccountPage : ContentPage
{
	private readonly AuthService _authService;
	private readonly UserService _userService;
	public AccountPage(AuthService authService, UserService userService)
	{
		_userService = userService;
		_authService = authService;
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		LoadUserData();
    }

    private void BtnLogoutClicked(object sender, EventArgs e)
    {
		Logout();
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

		LabelUsername.Text = response.User?.Username;
	}
}