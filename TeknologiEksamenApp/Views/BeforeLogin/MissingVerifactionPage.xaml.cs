namespace TeknologiEksamenApp.Views.BeforeLogin;

public partial class MissingVerifactionPage : ContentPage
{
	public MissingVerifactionPage()
	{
		InitializeComponent();
	}

    private void BtnReturnClicked(object sender, EventArgs e)
    {
		Shell.Current.GoToAsync("..");
    }
}