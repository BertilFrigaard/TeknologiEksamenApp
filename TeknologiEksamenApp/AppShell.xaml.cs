using TeknologiEksamenApp.Views.BeforeLogin;

namespace TeknologiEksamenApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(SignupPage), typeof(SignupPage));
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(MissingVerifactionPage), typeof(MissingVerifactionPage));

        CurrentItem = new ShellContent
        {
            ContentTemplate = new DataTemplate(typeof(LoadingPage)),
            Route = nameof(LoadingPage),
        };
    }
}
