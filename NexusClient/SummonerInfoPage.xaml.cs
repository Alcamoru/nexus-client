using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NexusClient;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SummonerInfoPage : Page
{
    public SummonerInfoPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var summonerName = e.Parameter.ToString();
        base.OnNavigatedTo(e);
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(WelcomePage));
    }
}