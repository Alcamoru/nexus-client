using Camille.RiotGames.MatchV5;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NexusClient;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MatchDetail : Page
{
    public MatchDetail()
    {
        InitializeComponent();
    }

    private Match MatchInfo { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        MatchInfo = (Match)e.Parameter;
        base.OnNavigatedTo(e);
    }

    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.GoBack();
    }
}