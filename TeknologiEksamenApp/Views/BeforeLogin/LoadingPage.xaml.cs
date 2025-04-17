namespace TeknologiEksamenApp.Views.BeforeLogin;

public partial class LoadingPage : ContentPage
{
	public LoadingPage()
	{
		InitializeComponent();
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await Shell.Current.GoToAsync(nameof(OnboardingPage));
    }
}