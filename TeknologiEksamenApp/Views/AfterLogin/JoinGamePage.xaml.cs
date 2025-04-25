namespace TeknologiEksamenApp.Views.AfterLogin;

public partial class JoinGamePage : ContentPage
{
	public JoinGamePage()
	{
		InitializeComponent();
	}

    private void BtnJoinClicked(object sender, EventArgs e)
    {

    }

    private async void BtnReturnClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}