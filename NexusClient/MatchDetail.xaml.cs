using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
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

    private RiotGamesApi Api { get; set; }

    private Match MatchInfo { get; set; }
    private Summoner LolSummoner { get; set; }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not List<object> parameters) return;
        Api = (RiotGamesApi)parameters.ElementAt(0);
        MatchInfo = (Match)parameters.ElementAt(1);
        LolSummoner = (Summoner)parameters.ElementAt(2);
        SetMatchTimeline();
        base.OnNavigatedTo(e);
    }

    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.GoBack();
    }

    private string GetChampionNameByPuuid(string puuid)
    {
        var championName = "";
        foreach (var participant in MatchInfo.Info.Participants)
            if (participant.Puuid == puuid)
                championName = participant.ChampionName;

        return championName;
    }

    private void SetMatchTimeline()
    {
        var timeline = Api.MatchV5().GetTimeline(RegionalRoute.EUROPE, MatchInfo.Metadata.MatchId);
        var summonerParticipantId = 0;
        foreach (var participant in timeline!.Info.Participants!)
        {
            if (participant.Puuid == LolSummoner.Puuid) summonerParticipantId = participant.ParticipantId;
            ;
        }


        var elementNumber = 0;
        foreach (var timelineInfoFrame in timeline.Info.Frames)
        foreach (var e in timelineInfoFrame.Events)
        {
            Debug.WriteLine(e);
            if (e.Type == "WARD_PLACED" && e.CreatorId == summonerParticipantId)
            {
                var frameInfoStackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                var creatorPuuid = "";

                foreach (var participant in timeline.Info.Participants)
                    if (participant.ParticipantId == e.CreatorId)
                        creatorPuuid = participant.Puuid;


                var championIcon = new Image
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Source = new BitmapImage(new Uri(
                        $"http://ddragon.leagueoflegends.com/cdn/13.22.1/img/champion/{GetChampionNameByPuuid(creatorPuuid)}.png")),
                    Width = 40
                };

                var wardImage = new Image
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 40,
                    Source = new BitmapImage(new Uri("https://opgg-static.akamaized.net/meta/images/lol/item/3340.png"))
                };

                var wardPlacedTextBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    HorizontalTextAlignment = TextAlignment.Center,
                    Text = " a placé une balise",
                    FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                };

                frameInfoStackPanel.Children.Add(championIcon);
                frameInfoStackPanel.Children.Add(wardPlacedTextBlock);
                frameInfoStackPanel.Children.Add(wardImage);
                Grid.SetColumn(frameInfoStackPanel, 1);
                Grid.SetRow(frameInfoStackPanel, elementNumber);
                elementNumber += 1;

                MatchTimelineGrid.Children.Add(frameInfoStackPanel);
                var row = new RowDefinition { Height = new GridLength(75, GridUnitType.Pixel) };
                MatchTimelineGrid.RowDefinitions.Add(row);
            }

            if (e.Type == "CHAMPION_KILL")
                if (e.KillerId == summonerParticipantId)
                {
                    var frameInfoStackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    var killerPuuid = "";
                    var victimPuuid = "";

                    foreach (var participant in timeline.Info.Participants)
                    {
                        if (participant.ParticipantId == e.KillerId) killerPuuid = participant.Puuid;

                        if (participant.ParticipantId == e.VictimId) victimPuuid = participant.Puuid;
                    }


                    var killerChampionIcon = new Image
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.22.1/img/champion/{GetChampionNameByPuuid(killerPuuid)}.png")),
                        Width = 40
                    };

                    var victimChampionIcon = new Image
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.22.1/img/champion/{GetChampionNameByPuuid(victimPuuid)}.png")),
                        Width = 40
                    };

                    var wardPlacedTextBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = " a eliminé",
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    frameInfoStackPanel.Children.Add(killerChampionIcon);
                    frameInfoStackPanel.Children.Add(wardPlacedTextBlock);
                    frameInfoStackPanel.Children.Add(victimChampionIcon);
                    Grid.SetColumn(frameInfoStackPanel, 1);
                    Grid.SetRow(frameInfoStackPanel, elementNumber);
                    elementNumber += 1;

                    MatchTimelineGrid.Children.Add(frameInfoStackPanel);
                    var row = new RowDefinition { Height = new GridLength(75, GridUnitType.Pixel) };
                    MatchTimelineGrid.RowDefinitions.Add(row);
                }
        }
    }
}