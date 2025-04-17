using TeknologiEksamenApp.Views.BeforeLogin;
using TeknologiEksamenApp.Views.AfterLogin;

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
        
        Routing.RegisterRoute(nameof(AccountPage), typeof(AccountPage));

        CurrentItem = new ShellContent
        {
            ContentTemplate = new DataTemplate(typeof(LoadingPage)),
            Route = nameof(LoadingPage),
        };
    }
}
