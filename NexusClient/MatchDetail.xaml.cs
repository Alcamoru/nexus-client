using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
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
using Team = Camille.RiotGames.Enums.Team;


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
        SetParticipantsGrid();
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

    private void SetParticipantsGrid()
    {

        List<Participant> team1 = new List<Participant>();
        List<Participant> team2 = new List<Participant>();

        foreach (Participant participant in MatchInfo.Info.Participants)
        {
            if (participant.TeamId == Team.Blue)
            {
                team1.Add(participant);
            }
            else
            {
                team2.Add(participant);
            }
        }

        int row = 0;
        foreach (Participant participant in team1)
        {
            switch (participant.Role)
            {
                case "SOLO":
                    row = 0;
                    break;
                case "NONE":
                    row = 1;
                    break;
                case "MIDDLE":
                    row = 2;
                    break;
                case "CARRY":
                    row = 3;
                    break;
                case "SUPPORT":
                    row = 4;
                    break;

            }

            Grid participantGrid = new Grid()
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 217, 217, 217)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10)
            };

            ColumnDefinition col1 = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition col2 = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition col3 = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition col4 = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition col5 = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            ColumnDefinition col6 = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };
            participantGrid.ColumnDefinitions.Add(col1);
            participantGrid.ColumnDefinitions.Add(col2);
            participantGrid.ColumnDefinitions.Add(col3);
            participantGrid.ColumnDefinitions.Add(col4);
            participantGrid.ColumnDefinitions.Add(col5);
            participantGrid.ColumnDefinitions.Add(col6);

            var championIcon = new Image
            {
                Source = new BitmapImage(new Uri(
                    $"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/champion/{participant.ChampionName}.png")),
                Width = 40
            };

            var championNameTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = participant.ChampionName,
                FontSize = 20,
                FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
            };

            Grid.SetColumn(championIcon, 0);
            participantGrid.Children.Add(championIcon);

            Grid.SetColumn(championNameTextBlock, 1);
            participantGrid.Children.Add(championNameTextBlock);

            var summonerChampionGrid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var summonerChampionGridColumn1 = new ColumnDefinition
                        { Width = new GridLength(30, GridUnitType.Pixel) };
                    var summonerChampionGridColumn2 = new ColumnDefinition
                        { Width = new GridLength(30, GridUnitType.Pixel) };

                    summonerChampionGrid.ColumnDefinitions.Add(summonerChampionGridColumn1);
                    summonerChampionGrid.ColumnDefinitions.Add(summonerChampionGridColumn2);

                    var summonerChampionGridRow1 = new RowDefinition
                        { Height = new GridLength(30, GridUnitType.Pixel) };
                    var summonerChampionGridRow2 = new RowDefinition
                        { Height = new GridLength(30, GridUnitType.Pixel) };

                    summonerChampionGrid.RowDefinitions.Add(summonerChampionGridRow1);
                    summonerChampionGrid.RowDefinitions.Add(summonerChampionGridRow2);

                    var sumsCorrespondences = new Dictionary<string, string>
                    {
                        { "21", "SummonerBarrier" },
                        { "1", "SummonerBoost" },
                        { "2202", "SummonerCherryFlash" },
                        { "2201", "SummonerCherryHold" },
                        { "14", "SummonerDot" },
                        { "3", "SummonerExhaust" },
                        { "4", "SummonerFlash" },
                        { "6", "SummonerHaste" },
                        { "7", "SummonerHeal" },
                        { "13", "SummonerMana" },
                        { "30", "SummonerPoroRecall" },
                        { "31", "SummonerPoroThrow" },
                        { "11", "SummonerSmite" },
                        { "39", "SummonerSnowURFSnowball_Mark" },
                        { "32", "SummonerSnowball" },
                        { "12", "SummonerTeleport" },
                        { "54", "Summoner_UltBookPlaceholder" },
                        { "55", "Summoner_UltBookSmitePlaceholder" }
                    };

                    var mainPerksCorrespondences = new Dictionary<string, List<string>>
                    {
                        { "8112", new List<string> { "Domination", "Electrocute" } },
                        { "8124", new List<string> { "Domination", "Predator" } },
                        { "8128", new List<string> { "Domination", "DarkHarvest" } },
                        { "9923", new List<string> { "Domination", "HailOfBlades" } },
                        { "8351", new List<string> { "Inspiration", "GlacialAugment" } },
                        { "8360", new List<string> { "Inspiration", "UnsealedSpellbook" } },
                        { "8369", new List<string> { "Inspiration", "FirstStrike" } },
                        { "8005", new List<string> { "Precision", "PressTheAttack" } },
                        { "8008", new List<string> { "Precision", "LethalTempo" } },
                        { "8021", new List<string> { "Precision", "FleetFootwork" } },
                        { "8010", new List<string> { "Precision", "Conqueror" } },
                        { "8437", new List<string> { "Resolve", "GraspOfTheUndying" } },
                        { "8439", new List<string> { "Resolve", "VeteranAftershock" } },
                        { "8465", new List<string> { "Resolve", "Guardian" } },
                        { "8214", new List<string> { "Sorcery", "SummonAery" } },
                        { "8229", new List<string> { "Sorcery", "ArcaneComet" } },
                        { "8230", new List<string> { "Sorcery", "PhaseRush" } }
                    };

                    var perksCategories = new Dictionary<string, string>
                    {
                        { "8100", "perk-images/Styles/7200_Domination.png" },
                        { "8300", "perk-images/Styles/7203_Whimsy.png" },
                        { "8000", "perk-images/Styles/7201_Precision.png" },
                        { "8400", "perk-images/Styles/7204_Resolve.png" },
                        { "8200", "perk-images/Styles/7202_Sorcery.png" }
                    };


                    var firstSummonerSpellImage = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/spell/{sumsCorrespondences[participant.Summoner1Id.ToString()]}.png"))
                    };

                    Grid.SetColumn(firstSummonerSpellImage, 0);
                    Grid.SetRow(firstSummonerSpellImage, 0);
                    summonerChampionGrid.Children.Add(firstSummonerSpellImage);

                    var secondSummonerSpellImage = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/spell/{sumsCorrespondences[participant.Summoner2Id.ToString()]}.png"))
                    };

                    Grid.SetColumn(secondSummonerSpellImage, 0);
                    Grid.SetRow(secondSummonerSpellImage, 1);
                    summonerChampionGrid.Children.Add(secondSummonerSpellImage);


                    var mainRune = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"https://ddragon.leagueoflegends.com/cdn/img/perk-images/Styles/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][0]}/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][1]}/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][1]}.png"))
                    };

                    Grid.SetColumn(mainRune, 1);
                    Grid.SetRow(mainRune, 0);
                    summonerChampionGrid.Children.Add(mainRune);


                    var secondaryRune = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"https://ddragon.leagueoflegends.com/cdn/img/{perksCategories[participant.Perks.Styles[1].Style.ToString()]}"))
                    };

                    Grid.SetColumn(secondaryRune, 1);
                    Grid.SetRow(secondaryRune, 1);
                    summonerChampionGrid.Children.Add(secondaryRune);

            Grid.SetColumn(summonerChampionGrid, 2);
            participantGrid.Children.Add(summonerChampionGrid);

            Grid.SetColumn(participantGrid, 0);
            Grid.SetRow(participantGrid, row);
            participantsGrid.Children.Add(participantGrid);
        }
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