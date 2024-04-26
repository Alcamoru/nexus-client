using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Newtonsoft.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.


namespace NexusClient;

/// <summary>
///     Represents a page that displays information about a summoner.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
    }

    private RiotGamesApi Api { get; set; }

    private Summoner LolSummoner { get; set; }
    private RegionalRoute SummonerRegionalRoute { get; set; }

    private PlatformRoute SummonerPlatformRoute { get; set; }
    private UtilisMethods Methods { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not List<object> parameters) return;
        Api = (RiotGamesApi)parameters.ElementAt(0);
        LolSummoner = (Summoner)parameters.ElementAt(1);
        SummonerRegionalRoute = (RegionalRoute)parameters.ElementAt(2);
        SummonerPlatformRoute = (PlatformRoute)parameters.ElementAt(3);
    }

    private void NavigationView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
        else
        {
            Frame.GoBack();
        }
    }

    private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string selectedItemTag = ((string)selectedItem.Tag);
            var parametersList = new List<object>
            {
                Api,
                LolSummoner,
                SummonerRegionalRoute,
                SummonerPlatformRoute
            };
            switch (selectedItemTag)
            {
                case "Accueil":
                    sender.Header = "Accueil";
                    ContentFrame.Navigate(typeof(WelcomePage), parametersList, new DrillInNavigationTransitionInfo());
                    break;
                case "Profil":
                    sender.Header = "Profil";
                    ContentFrame.Navigate(typeof(ProfilePage), parametersList, new DrillInNavigationTransitionInfo());
                    break;
                case "Tier List":
                    sender.Header = "Tier List";
                    ContentFrame.Navigate(typeof(TierListPage), parametersList, new DrillInNavigationTransitionInfo());
                    break;
                case "Builds":
                    sender.Header = "Builds";
                    ContentFrame.Navigate(typeof(BuildsPage), parametersList, new DrillInNavigationTransitionInfo());
                    break;
            }
    }
}