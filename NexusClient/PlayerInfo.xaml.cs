using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NexusClient;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PlayerInfo : Page
{
    public PlayerInfo()
    {
        InitializeComponent();
    }

    private RiotGamesApi Api { get; set; }

    private Summoner LolSummoner { get; set; }

    private Summoner SummonerInfo { get; set; }

    private RegionalRoute SummonerRegionalRoute { get; set; }

    private PlatformRoute SummonerPlatformRoute { get; set; }

    private UtilisMethods Methods { get; set; }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not List<object> parameters) return;
        Api = (RiotGamesApi)parameters.ElementAt(0);
        SummonerInfo = (Summoner)parameters.ElementAt(1);
        LolSummoner = (Summoner)parameters.ElementAt(2);
        SummonerRegionalRoute = (RegionalRoute)parameters.ElementAt(3);
        SummonerPlatformRoute = (PlatformRoute)parameters.ElementAt(4);
        Methods = new UtilisMethods(Api, LolSummoner, SummonerRegionalRoute, SummonerPlatformRoute);
        base.OnNavigatedTo(e);
    }


    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.GoBack(new DrillInNavigationTransitionInfo());
    }
}