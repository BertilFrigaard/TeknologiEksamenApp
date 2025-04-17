namespace TeknologiEksamenApp.Views.BeforeLogin;

public partial class LoadingPage : ContentPage
{
	public LoadingPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Task.Run(async () =>
        {
            await Task.Delay(2000);
            await Shell.Current.GoToAsync(nameof(OnboardingPage));
        });
    }
}