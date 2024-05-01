using System.Collections.Generic;
using System.Linq;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.


namespace NexusClient;

/// <summary>
///     Represents a page that displays information about a summoner.
/// </summary>
public sealed partial class ProfilePage : Page
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    private Summoner SummonerInfo { get; set; }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not List<object> parameters) return;
        SummonerInfo = (Summoner)parameters.ElementAt(1);
        base.OnNavigatedTo(e);
    }
}