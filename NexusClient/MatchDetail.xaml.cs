using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.UI;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Newtonsoft.Json;
using SkiaSharp;
using Team = Camille.RiotGames.Enums.Team;
using static NexusClient.UtilisMethods;


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

    private Summoner LolSummoner { get; set; }

    private Match MatchInfo { get; set; }

    private RegionalRoute SummonerRegionalRoute { get; set; }

    private PlatformRoute SummonerPlatformRoute { get; set; }

    private List<Participant> Team1 { get; set; }

    private List<Participant> Team2 { get; set; }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not List<object> parameters) return;
        Api = (RiotGamesApi)parameters.ElementAt(0);
        MatchInfo = (Match)parameters.ElementAt(1);
        LolSummoner = (Summoner)parameters.ElementAt(2);
        SummonerRegionalRoute = (RegionalRoute)parameters.ElementAt(3);
        SummonerPlatformRoute = (PlatformRoute)parameters.ElementAt(4);
        SetMatchTimeline();
        SetParticipantsGrid();
        Team1 = new List<Participant>();
        Team2 = new List<Participant>();
        SetTeams();
        SetMatchGraph();
        base.OnNavigatedTo(e);
    }

    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.GoBack(new DrillInNavigationTransitionInfo());
    }

    private string GetChampionNameByPuuid(string puuid)
    {
        var championName = "";
        foreach (var participant in MatchInfo.Info.Participants)
            if (participant.Puuid == puuid)
                championName = participant.ChampionName;

        return championName;
    }

    /// <summary>
    ///     Sets up the participants grid by adding participants' information and styling.
    /// </summary>
    /// <returns>None</returns>
    private void SetParticipantsGrid()
    {
        var team1 = new List<Participant>();
        var team2 = new List<Participant>();

        foreach (var participant in MatchInfo.Info.Participants)
        {
            if (participant.SummonerName == LolSummoner.Name)
            {
                if (participant.Win)
                {
                    SummonerWonTextBlock.Text = "Victoire";
                    SummonerWonTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96));
                }
                else
                {
                    SummonerWonTextBlock.Text = "Défaite";
                    SummonerWonTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6));
                }
            }

            if (participant.TeamId == Team.Blue)
                team1.Add(participant);
            else
                team2.Add(participant);
        }

        var row = 0;

        var team1BestDamages = 0;
        foreach (var participant in team1)
            if (participant.TotalDamageDealtToChampions > team1BestDamages)
                team1BestDamages = participant.TotalDamageDealtToChampions;
        var team2BestDamages = 0;
        foreach (var participant in team2)
            if (participant.TotalDamageDealtToChampions > team2BestDamages)
                team2BestDamages = participant.TotalDamageDealtToChampions;

        foreach (var participant in team1)
        {
            switch (participant.TeamPosition)
            {
                case "TOP":
                    row = 0;
                    break;
                case "JUNGLE":
                    row = 1;
                    break;
                case "MIDDLE":
                    row = 2;
                    break;
                case "BOTTOM":
                    row = 3;
                    break;
                case "UTILITY":
                    row = 4;
                    break;
            }

            var participantGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 217, 217, 217)),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(2),
                Padding = new Thickness(5)
            };

            if (participant.SummonerName == LolSummoner.Name)
                participantGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96));
            else
                participantGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185));

            participantGrid.BorderThickness = new Thickness(2);

            var col1 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col2 = new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) };
            var col3 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col4 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col5 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col6 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            participantGrid.ColumnDefinitions.Add(col1);
            participantGrid.ColumnDefinitions.Add(col2);
            participantGrid.ColumnDefinitions.Add(col3);
            participantGrid.ColumnDefinitions.Add(col4);
            participantGrid.ColumnDefinitions.Add(col5);
            participantGrid.ColumnDefinitions.Add(col6);


            var source =
                $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/champion/{participant.ChampionName}.png";
            var championIcon = GetImage(source, 30, 15);

            var championIconViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = championIcon
            };

            var championNameViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var championNameTextBlock = SetText(participant.SummonerName,
                20, Color.FromArgb(255, 52, 73, 94));

            championNameViewbox.Child = championNameTextBlock;

            Grid.SetColumn(championIconViewbox, 0);
            participantGrid.Children.Add(championIconViewbox);

            Grid.SetColumn(championNameViewbox, 1);
            participantGrid.Children.Add(championNameViewbox);

            var summonersViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var summonerChampionGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var summonerChampionGridColumn1 = new ColumnDefinition
                { Width = new GridLength(15, GridUnitType.Pixel) };
            var summonerChampionGridColumn2 = new ColumnDefinition
                { Width = new GridLength(15, GridUnitType.Pixel) };

            summonerChampionGrid.ColumnDefinitions.Add(summonerChampionGridColumn1);
            summonerChampionGrid.ColumnDefinitions.Add(summonerChampionGridColumn2);

            var summonerChampionGridRow1 = new RowDefinition
                { Height = new GridLength(15, GridUnitType.Pixel) };
            var summonerChampionGridRow2 = new RowDefinition
                { Height = new GridLength(15, GridUnitType.Pixel) };

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

            source =
                $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/spell/{sumsCorrespondences[participant.Summoner1Id.ToString()]}.png";
            var firstSummonerSpellImage = GetImage(source);

            Grid.SetColumn(firstSummonerSpellImage, 0);
            Grid.SetRow(firstSummonerSpellImage, 0);
            summonerChampionGrid.Children.Add(firstSummonerSpellImage);

            source =
                $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/spell/{sumsCorrespondences[participant.Summoner2Id.ToString()]}.png";
            var secondSummonerSpellImage = GetImage(source);

            Grid.SetColumn(secondSummonerSpellImage, 0);
            Grid.SetRow(secondSummonerSpellImage, 1);
            summonerChampionGrid.Children.Add(secondSummonerSpellImage);


            var perksJson =
                File.ReadAllText(
                    @"C:\Users\alcam\OneDrive\Developpement\nexus-client\NexusClient\NexusClient\Assets\loldata\14.1.1\data\fr_FR\runesReforged.json");
            var runesClass = JsonConvert.DeserializeObject<List<PerksClass.Root>>(perksJson);

            var firstPerkIcon = "";
            var secondPerkIcon = "";

            foreach (var root in runesClass)
            {
                if (root.id == participant.Perks.Styles[0].Style)
                    foreach (var rune in root.slots[0].runes)
                        if (rune.id == participant.Perks.Styles[0].Selections[0].Perk)
                            firstPerkIcon = rune.icon;

                if (root.id == participant.Perks.Styles[1].Style) secondPerkIcon = root.icon;
            }


            source = $"https://ddragon.leagueoflegends.com/cdn/img/{firstPerkIcon}";
            var mainRune = GetImage(source);
            Grid.SetColumn(mainRune, 1);
            Grid.SetRow(mainRune, 0);
            summonerChampionGrid.Children.Add(mainRune);

            source = $"https://ddragon.leagueoflegends.com/cdn/img/{secondPerkIcon}";
            var secondaryRune = GetImage(source);

            Grid.SetColumn(secondaryRune, 1);
            Grid.SetRow(secondaryRune, 1);
            summonerChampionGrid.Children.Add(secondaryRune);

            summonersViewbox.Child = summonerChampionGrid;

            Grid.SetColumn(summonersViewbox, 2);
            participantGrid.Children.Add(summonersViewbox);

            var csChampionTextBlock = SetText(
                $"{participant.TotalMinionsKilled + participant.TotalAllyJungleMinionsKilled + participant.TotalEnemyJungleMinionsKilled} CS",
                15, Color.FromArgb(255, 52, 73, 94), stretch: Stretch.Uniform);

            Grid.SetColumn(csChampionTextBlock, 3);
            participantGrid.Children.Add(csChampionTextBlock);

            var team1Rectangle = new Rectangle
            {
                Width = 50,
                Fill = new SolidColorBrush(Colors.White),
                Height = 7
            };

            var team1Border = new Border
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = team1Rectangle,
                CornerRadius = new CornerRadius(3)
            };


            var width = participant.TotalDamageDealtToChampions / (float)team1BestDamages * 50;

            var participant1Rectangle = new Rectangle
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = width,
                Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
                Height = 7
            };

            var participant1Border = new Border
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = participant1Rectangle,
                CornerRadius = new CornerRadius(3)
            };

            var innerGrid1 = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            innerGrid1.Children.Add(team1Border);
            innerGrid1.Children.Add(participant1Border);

            Grid.SetColumn(innerGrid1, 4);
            participantGrid.Children.Add(innerGrid1);

            var kdaChampionTextBlock = SetText($"{participant.Kills} | {participant.Deaths} | {participant.Assists}",
                15, Color.FromArgb(255, 52, 73, 94), stretch: Stretch.Uniform);

            Grid.SetColumn(kdaChampionTextBlock, 5);
            participantGrid.Children.Add(kdaChampionTextBlock);

            Grid.SetColumn(participantGrid, 0);
            Grid.SetRow(participantGrid, row);
            ParticipantsGrid.Children.Add(participantGrid);
        }

        foreach (var participant in team2)
        {
            switch (participant.TeamPosition)
            {
                case "TOP":
                    row = 0;
                    break;
                case "JUNGLE":
                    row = 1;
                    break;
                case "MIDDLE":
                    row = 2;
                    break;
                case "BOTTOM":
                    row = 3;
                    break;
                case "UTILITY":
                    row = 4;
                    break;
            }

            var participantGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 217, 217, 217)),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(2),
                Padding = new Thickness(5)
            };

            if (participant.SummonerName == LolSummoner.Name)
                participantGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96));
            else
                participantGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6));

            participantGrid.BorderThickness = new Thickness(2);

            var col1 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col2 = new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) };
            var col3 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col4 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col5 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col6 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            participantGrid.ColumnDefinitions.Add(col1);
            participantGrid.ColumnDefinitions.Add(col2);
            participantGrid.ColumnDefinitions.Add(col3);
            participantGrid.ColumnDefinitions.Add(col4);
            participantGrid.ColumnDefinitions.Add(col5);
            participantGrid.ColumnDefinitions.Add(col6);


            var source =
                $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/champion/{participant.ChampionName}.png";
            var championIcon = GetImage(source, 30, 15);

            var championIconViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = championIcon
            };

            var championNameViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var championNameTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromArgb(255, 52, 73, 94)),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = participant.SummonerName,
                FontSize = 20,
                FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
            };

            championNameViewbox.Child = championNameTextBlock;

            Grid.SetColumn(championIconViewbox, 0);
            participantGrid.Children.Add(championIconViewbox);

            Grid.SetColumn(championNameViewbox, 1);
            participantGrid.Children.Add(championNameViewbox);

            var summonersViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var summonerChampionGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var summonerChampionGridColumn1 = new ColumnDefinition
                { Width = new GridLength(15, GridUnitType.Pixel) };
            var summonerChampionGridColumn2 = new ColumnDefinition
                { Width = new GridLength(15, GridUnitType.Pixel) };

            summonerChampionGrid.ColumnDefinitions.Add(summonerChampionGridColumn1);
            summonerChampionGrid.ColumnDefinitions.Add(summonerChampionGridColumn2);

            var summonerChampionGridRow1 = new RowDefinition
                { Height = new GridLength(15, GridUnitType.Pixel) };
            var summonerChampionGridRow2 = new RowDefinition
                { Height = new GridLength(15, GridUnitType.Pixel) };

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


            source =
                $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/spell/{sumsCorrespondences[participant.Summoner1Id.ToString()]}.png";
            var firstSummonerSpellImage = GetImage(source);

            Grid.SetColumn(firstSummonerSpellImage, 0);
            Grid.SetRow(firstSummonerSpellImage, 0);
            summonerChampionGrid.Children.Add(firstSummonerSpellImage);

            source =
                $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/spell/{sumsCorrespondences[participant.Summoner2Id.ToString()]}.png";
            var secondSummonerSpellImage = GetImage(source);

            Grid.SetColumn(secondSummonerSpellImage, 0);
            Grid.SetRow(secondSummonerSpellImage, 1);
            summonerChampionGrid.Children.Add(secondSummonerSpellImage);


            var perksJson =
                File.ReadAllText(
                    @"C:\Users\alcam\OneDrive\Developpement\nexus-client\NexusClient\NexusClient\Assets\loldata\14.1.1\data\fr_FR\runesReforged.json");
            var runesClass = JsonConvert.DeserializeObject<List<PerksClass.Root>>(perksJson);

            var firstPerkIcon = "";
            var secondPerkIcon = "";

            foreach (var root in runesClass)
            {
                if (root.id == participant.Perks.Styles[0].Style)
                    foreach (var rune in root.slots[0].runes)
                        if (rune.id == participant.Perks.Styles[0].Selections[0].Perk)
                            firstPerkIcon = rune.icon;

                if (root.id == participant.Perks.Styles[1].Style) secondPerkIcon = root.icon;
            }


            source =
                $"https://ddragon.leagueoflegends.com/cdn/img/{firstPerkIcon}";

            var mainRune = GetImage(source);

            Grid.SetColumn(mainRune, 1);
            Grid.SetRow(mainRune, 0);
            summonerChampionGrid.Children.Add(mainRune);


            source = $"https://ddragon.leagueoflegends.com/cdn/img/{secondPerkIcon}";
            var secondaryRune = GetImage(source);

            Grid.SetColumn(secondaryRune, 1);
            Grid.SetRow(secondaryRune, 1);
            summonerChampionGrid.Children.Add(secondaryRune);

            summonersViewbox.Child = summonerChampionGrid;

            Grid.SetColumn(summonersViewbox, 2);
            participantGrid.Children.Add(summonersViewbox);


            var team2Rectangle = new Rectangle
            {
                Width = 50,
                Fill = new SolidColorBrush(Colors.White),
                Height = 7
            };

            var team2Border = new Border
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = team2Rectangle,
                CornerRadius = new CornerRadius(3)
            };


            var width = participant.TotalDamageDealtToChampions / (float)team2BestDamages * 50;

            var participant2Rectangle = new Rectangle
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = width,
                Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
                Height = 7
            };

            var participant2Border = new Border
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = participant2Rectangle,
                CornerRadius = new CornerRadius(3)
            };

            var innerGrid2 = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            innerGrid2.Children.Add(team2Border);
            innerGrid2.Children.Add(participant2Border);

            Grid.SetColumn(innerGrid2, 4);
            participantGrid.Children.Add(innerGrid2);

            var csChampionTextBlock = SetText(
                $"{participant.TotalMinionsKilled + participant.TotalAllyJungleMinionsKilled + participant.TotalEnemyJungleMinionsKilled} CS",
                15, Color.FromArgb(255, 52, 73, 94), stretch: Stretch.Uniform);


            Grid.SetColumn(csChampionTextBlock, 3);
            participantGrid.Children.Add(csChampionTextBlock);

            var kdaChampionTextBlock = SetText($"{participant.Kills} | {participant.Deaths} | {participant.Assists}",
                15, Color.FromArgb(255, 52, 73, 94), stretch: Stretch.Uniform);

            Grid.SetColumn(kdaChampionTextBlock, 5);
            participantGrid.Children.Add(kdaChampionTextBlock);


            Grid.SetColumn(participantGrid, 2);
            Grid.SetRow(participantGrid, row);
            ParticipantsGrid.Children.Add(participantGrid);
        }
    }

    private void SetTeams()
    {
        foreach (var participant in MatchInfo.Info.Participants)
            if (participant.TeamId == Team.Blue)
                Team1.Add(participant);
            else
                Team2.Add(participant);
    }


    /// <summary>
    ///     Sets the match timeline by adding visual elements to the MatchTimelineGrid.
    /// </summary>
    /// <returns>None</returns>
    private void SetMatchTimeline()
    {
        var elementNumber = 0;

        var precedentEventIsPositive = true;


        SetPositiveEvent();
        elementNumber += 1;

        var timeline = Api.MatchV5().GetTimeline(SummonerRegionalRoute, MatchInfo.Metadata.MatchId);
        var summonerParticipantId = 0;
        foreach (var participant in timeline!.Info.Participants!)
            if (participant.Puuid == LolSummoner.Puuid)
                summonerParticipantId = participant.ParticipantId;


        void SetPrecedentEvent()
        {
            if (precedentEventIsPositive)
            {
                var rectangleGreenTop = new Rectangle
                {
                    Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
                    Height = 75,
                    Width = 4,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Grid.SetColumn(rectangleGreenTop, 0);
                Grid.SetRow(rectangleGreenTop, elementNumber);
                MatchTimelineGrid.Children.Add(rectangleGreenTop);
            }
            else
            {
                var rectangleRedTop = new Rectangle
                {
                    Fill = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6)),
                    Height = 75,
                    Width = 4,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Grid.SetColumn(rectangleRedTop, 0);
                Grid.SetRow(rectangleRedTop, elementNumber);
                MatchTimelineGrid.Children.Add(rectangleRedTop);
            }
        }

        void SetPositiveEvent()
        {
            var ellipseGreen = new Ellipse
            {
                Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
                Height = 10,
                Width = 10,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var rectangleGreenBot = new Rectangle
            {
                Margin = new Thickness(0, 37, 0, 0),
                Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
                Height = 75,
                Width = 4,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Grid.SetColumn(ellipseGreen, 0);
            Grid.SetRow(ellipseGreen, elementNumber);
            MatchTimelineGrid.Children.Add(ellipseGreen);
            Grid.SetColumn(rectangleGreenBot, 0);
            Grid.SetRow(rectangleGreenBot, elementNumber);
            MatchTimelineGrid.Children.Add(rectangleGreenBot);
        }

        void SetNegativeEvent()
        {
            var ellipseRed = new Ellipse
            {
                Fill = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6)),
                Height = 10,
                Width = 10,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var rectangleRedBot = new Rectangle
            {
                Margin = new Thickness(0, 37, 0, 0),
                Fill = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6)),
                Height = 75,
                Width = 4,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Grid.SetColumn(ellipseRed, 0);
            Grid.SetRow(ellipseRed, elementNumber);
            MatchTimelineGrid.Children.Add(ellipseRed);
            Grid.SetColumn(rectangleRedBot, 0);
            Grid.SetRow(rectangleRedBot, elementNumber);
            MatchTimelineGrid.Children.Add(rectangleRedBot);
        }

        var startStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10, 0, 0, 0)
        };

        var startIcon = new Image
        {
            Width = 40,
            Source = new BitmapImage(
                new Uri("ms-appx:///Assets/media/bouton-de-lecture-video.png"))
        };

        var startTextBlock = SetText(" début du match",
            15, Colors.White);

        startStackPanel.Children.Add(startIcon);
        startStackPanel.Children.Add(startTextBlock);
        Grid.SetColumn(startStackPanel, 1);
        Grid.SetRow(startStackPanel, 0);

        MatchTimelineGrid.Children.Add(startStackPanel);
        MatchTimelineGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(75, GridUnitType.Pixel) });

        foreach (var timelineInfoFrame in timeline.Info.Frames)
        foreach (var e in timelineInfoFrame.Events)
        {
            if (e.Type == "WARD_PLACED" && e.CreatorId == summonerParticipantId)
            {
                SetPrecedentEvent();

                var frameInfoStackPanel = new StackPanel
                {
                    Margin = new Thickness(10, 0, 0, 0),
                    Orientation = Orientation.Horizontal
                };

                var creatorPuuid = "";

                foreach (var participant in timeline.Info.Participants)
                    if (participant.ParticipantId == e.CreatorId)
                        creatorPuuid = participant.Puuid;

                var source =
                    $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/champion/{GetChampionNameByPuuid(creatorPuuid)}.png";
                var championIcon = GetImage(source, 7, 40);
                championIcon.VerticalAlignment = VerticalAlignment.Center;


                source =
                    @"C:\Users\alcam\OneDrive\Developpement\nexus-client\NexusClient\NexusClient\Assets\loldata\14.1.1\img\item\3340.png";
                var wardImage = GetImage(source, 7, 40);
                wardImage.VerticalAlignment = VerticalAlignment.Center;

                var wardPlacedTextBlock = SetText(" a placé une balise",
                    15, Colors.White);

                frameInfoStackPanel.Children.Add(championIcon);
                frameInfoStackPanel.Children.Add(wardPlacedTextBlock);
                frameInfoStackPanel.Children.Add(wardImage);
                Grid.SetColumn(frameInfoStackPanel, 1);
                Grid.SetRow(frameInfoStackPanel, elementNumber);


                SetPositiveEvent();

                elementNumber += 1;

                MatchTimelineGrid.Children.Add(frameInfoStackPanel);
                var row = new RowDefinition { Height = new GridLength(75, GridUnitType.Pixel) };
                MatchTimelineGrid.RowDefinitions.Add(row);
                precedentEventIsPositive = true;
            }

            if (e.Type == "CHAMPION_KILL")
            {
                if (e.KillerId == summonerParticipantId)
                {
                    SetPrecedentEvent();

                    var frameInfoStackPanel = new StackPanel
                    {
                        Margin = new Thickness(10, 0, 0, 0),

                        Orientation = Orientation.Horizontal
                    };

                    var killerPuuid = "";
                    var victimPuuid = "";

                    foreach (var participant in timeline.Info.Participants)
                    {
                        if (participant.ParticipantId == e.KillerId) killerPuuid = participant.Puuid;

                        if (participant.ParticipantId == e.VictimId) victimPuuid = participant.Puuid;
                    }

                    var source =
                        $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/champion/{GetChampionNameByPuuid(killerPuuid)}.png";
                    var killerChampionIcon = GetImage(source, 7, 40);
                    killerChampionIcon.VerticalAlignment = VerticalAlignment.Center;

                    source =
                        $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/champion/{GetChampionNameByPuuid(victimPuuid)}.png";
                    var victimChampionIcon = GetImage(source, 7, 40);
                    victimChampionIcon.VerticalAlignment = VerticalAlignment.Center;

                    var eliminatedTextBlock = SetText(" a eliminé",
                        15, Colors.White);

                    frameInfoStackPanel.Children.Add(killerChampionIcon);
                    frameInfoStackPanel.Children.Add(eliminatedTextBlock);
                    frameInfoStackPanel.Children.Add(victimChampionIcon);
                    Grid.SetColumn(frameInfoStackPanel, 1);
                    Grid.SetRow(frameInfoStackPanel, elementNumber);

                    SetPositiveEvent();

                    elementNumber += 1;

                    MatchTimelineGrid.Children.Add(frameInfoStackPanel);
                    var row = new RowDefinition { Height = new GridLength(75, GridUnitType.Pixel) };
                    MatchTimelineGrid.RowDefinitions.Add(row);
                    precedentEventIsPositive = true;
                }

                if (e.VictimId == summonerParticipantId)
                {
                    SetPrecedentEvent();

                    var frameInfoStackPanel = new StackPanel
                    {
                        Margin = new Thickness(10, 0, 0, 0),
                        Orientation = Orientation.Horizontal
                    };

                    var killerPuuid = "";

                    foreach (var participant in timeline.Info.Participants)
                        if (participant.ParticipantId == e.KillerId)
                            killerPuuid = participant.Puuid;


                    var source =
                        $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/champion/{GetChampionNameByPuuid(killerPuuid)}.png";
                    var killerChampionIcon = GetImage(source, 7, 40);
                    killerChampionIcon.VerticalAlignment = VerticalAlignment.Center;

                    source =
                        $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/champion/{GetChampionNameByPuuid(LolSummoner.Puuid)}.png";
                    var victimChampionIcon = GetImage(source, 7, 40);
                    victimChampionIcon.VerticalAlignment = VerticalAlignment.Center;

                    var eliminatedTextBlock = SetText(" a eliminé",
                        15, Colors.White);

                    frameInfoStackPanel.Children.Add(killerChampionIcon);
                    frameInfoStackPanel.Children.Add(eliminatedTextBlock);
                    frameInfoStackPanel.Children.Add(victimChampionIcon);
                    Grid.SetColumn(frameInfoStackPanel, 1);
                    Grid.SetRow(frameInfoStackPanel, elementNumber);

                    SetNegativeEvent();

                    elementNumber += 1;

                    MatchTimelineGrid.Children.Add(frameInfoStackPanel);
                    var row = new RowDefinition { Height = new GridLength(75, GridUnitType.Pixel) };
                    MatchTimelineGrid.RowDefinitions.Add(row);
                    precedentEventIsPositive = false;
                }
            }


            if (e.AssistingParticipantIds != null)
                if (e.AssistingParticipantIds.Contains(summonerParticipantId))
                {
                    SetPrecedentEvent();

                    var frameInfoStackPanel = new StackPanel
                    {
                        Margin = new Thickness(10, 0, 0, 0),
                        Orientation = Orientation.Horizontal
                    };

                    var victimPuuid = "";

                    foreach (var participant in timeline.Info.Participants)
                        if (participant.ParticipantId == e.VictimId)
                            victimPuuid = participant.Puuid;


                    var source =
                        $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/champion/{GetChampionNameByPuuid(LolSummoner.Puuid)}.png";
                    var summonerChampionIcon = GetImage(source, 7, 40);
                    summonerChampionIcon.VerticalAlignment = VerticalAlignment.Center;

                    source =
                        $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/champion/{GetChampionNameByPuuid(victimPuuid)}.png";
                    var victimChampionIcon = GetImage(source, 7, 40);
                    victimChampionIcon.VerticalAlignment = VerticalAlignment.Center;


                    var eliminatedTextBlock = SetText(" a participé à l'élimination de ",
                        15, Colors.White);

                    frameInfoStackPanel.Children.Add(summonerChampionIcon);
                    frameInfoStackPanel.Children.Add(eliminatedTextBlock);
                    frameInfoStackPanel.Children.Add(victimChampionIcon);
                    Grid.SetColumn(frameInfoStackPanel, 1);
                    Grid.SetRow(frameInfoStackPanel, elementNumber);


                    SetPositiveEvent();

                    elementNumber += 1;

                    MatchTimelineGrid.Children.Add(frameInfoStackPanel);
                    var row = new RowDefinition { Height = new GridLength(75, GridUnitType.Pixel) };
                    MatchTimelineGrid.RowDefinitions.Add(row);
                    precedentEventIsPositive = true;
                }
        }

        var endStackPanel = new StackPanel
        {
            Margin = new Thickness(10, 0, 0, 0),
            Orientation = Orientation.Horizontal
        };

        var endIcon = new Image
        {
            Width = 40,
            Source = new BitmapImage(
                new Uri("ms-appx:///Assets/media/bouton-de-lecture-video.png"))
        };

        var endTextBlock = SetText(" fin du match",
            15, Colors.White);

        endStackPanel.Children.Add(endIcon);
        endStackPanel.Children.Add(endTextBlock);
        Grid.SetColumn(endStackPanel, 1);
        Grid.SetRow(endStackPanel, elementNumber);

        var ellipseGreen = new Ellipse
        {
            Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
            Height = 10,
            Width = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(ellipseGreen, 0);
        Grid.SetRow(ellipseGreen, elementNumber);
        MatchTimelineGrid.Children.Add(ellipseGreen);

        var rectangleGreenTop = new Rectangle
        {
            Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
            Height = 37,
            Width = 4,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(rectangleGreenTop, 0);
        Grid.SetRow(rectangleGreenTop, elementNumber);
        MatchTimelineGrid.Children.Add(rectangleGreenTop);

        MatchTimelineGrid.Children.Add(endStackPanel);
    }

    private void SetMatchGraph()
    {
        var timeline = Api.MatchV5().GetTimeline(SummonerRegionalRoute, MatchInfo.Metadata.MatchId)!;
        var team1Money = new ObservableCollection<int>();
        var team2Money = new ObservableCollection<int>();
        foreach (var frame in timeline.Info.Frames)
        {
            var totalTeam1Money = 0;
            foreach (var participant in Team1)
            {
                if (participant.ParticipantId == frame.ParticipantFrames.X1.ParticipantId)
                    totalTeam1Money += frame.ParticipantFrames.X1.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X2.ParticipantId)
                    totalTeam1Money += frame.ParticipantFrames.X2.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X3.ParticipantId)
                    totalTeam1Money += frame.ParticipantFrames.X3.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X4.ParticipantId)
                    totalTeam1Money += frame.ParticipantFrames.X4.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X5.ParticipantId)
                    totalTeam1Money += frame.ParticipantFrames.X5.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X6.ParticipantId)
                    totalTeam1Money += frame.ParticipantFrames.X6.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X7.ParticipantId)
                    totalTeam1Money += frame.ParticipantFrames.X7.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X8.ParticipantId)
                    totalTeam1Money += frame.ParticipantFrames.X8.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X9!.ParticipantId)
                    totalTeam1Money += frame.ParticipantFrames.X9.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X10!.ParticipantId)
                    totalTeam1Money += frame.ParticipantFrames.X10.TotalGold;
            }

            team1Money.Add(totalTeam1Money);


            var totalTeam2Money = 0;
            foreach (var participant in Team2)
            {
                if (participant.ParticipantId == frame.ParticipantFrames.X1.ParticipantId)
                    totalTeam2Money += frame.ParticipantFrames.X1.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X2.ParticipantId)
                    totalTeam2Money += frame.ParticipantFrames.X1.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X3.ParticipantId)
                    totalTeam2Money += frame.ParticipantFrames.X1.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X4.ParticipantId)
                    totalTeam2Money += frame.ParticipantFrames.X1.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X5.ParticipantId)
                    totalTeam2Money += frame.ParticipantFrames.X1.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X6.ParticipantId)
                    totalTeam2Money += frame.ParticipantFrames.X1.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X7.ParticipantId)
                    totalTeam2Money += frame.ParticipantFrames.X1.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X8.ParticipantId)
                    totalTeam2Money += frame.ParticipantFrames.X1.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X9!.ParticipantId)
                    totalTeam2Money += frame.ParticipantFrames.X1.TotalGold;
                if (participant.ParticipantId == frame.ParticipantFrames.X10!.ParticipantId)
                    totalTeam2Money += frame.ParticipantFrames.X1.TotalGold;
            }

            team2Money.Add(totalTeam2Money);
        }

        var team1Serie = new LineSeries<int>
        {
            Values = team1Money,
            Stroke = new SolidColorPaint(new SKColor(41, 128, 185)),
            GeometrySize = 2
        };
        var team2Serie = new LineSeries<int>
        {
            Values = team2Money,
            Stroke = new SolidColorPaint(new SKColor(235, 47, 6)),
            GeometrySize = 2
        };
        GoldChart.Series = new List<ISeries> { team1Serie, team2Serie };

        var summonerId = 1;
        foreach (var metadataParticipant in timeline.Metadata.Participants)
        {
            if (metadataParticipant == LolSummoner.Puuid) break;

            summonerId++;
        }


        var playerMinions = new ObservableCollection<int>();
        foreach (var frame in timeline.Info.Frames)
        {
            if (summonerId == frame.ParticipantFrames.X1.ParticipantId)
                playerMinions.Add(frame.ParticipantFrames.X1.MinionsKilled +
                                  frame.ParticipantFrames.X1.JungleMinionsKilled);
            if (summonerId == frame.ParticipantFrames.X2.ParticipantId)
                playerMinions.Add(frame.ParticipantFrames.X2.MinionsKilled +
                                  frame.ParticipantFrames.X1.JungleMinionsKilled);
            if (summonerId == frame.ParticipantFrames.X3.ParticipantId)
                playerMinions.Add(frame.ParticipantFrames.X3.MinionsKilled +
                                  frame.ParticipantFrames.X1.JungleMinionsKilled);
            if (summonerId == frame.ParticipantFrames.X4.ParticipantId)
                playerMinions.Add(frame.ParticipantFrames.X4.MinionsKilled +
                                  frame.ParticipantFrames.X1.JungleMinionsKilled);
            if (summonerId == frame.ParticipantFrames.X5.ParticipantId)
                playerMinions.Add(frame.ParticipantFrames.X5.MinionsKilled +
                                  frame.ParticipantFrames.X1.JungleMinionsKilled);
            if (summonerId == frame.ParticipantFrames.X6.ParticipantId)
                playerMinions.Add(frame.ParticipantFrames.X6.MinionsKilled +
                                  frame.ParticipantFrames.X1.JungleMinionsKilled);
            if (summonerId == frame.ParticipantFrames.X7.ParticipantId)
                playerMinions.Add(frame.ParticipantFrames.X7.MinionsKilled +
                                  frame.ParticipantFrames.X1.JungleMinionsKilled);
            if (summonerId == frame.ParticipantFrames.X8.ParticipantId)
                playerMinions.Add(frame.ParticipantFrames.X8.MinionsKilled +
                                  frame.ParticipantFrames.X1.JungleMinionsKilled);
            if (summonerId == frame.ParticipantFrames.X9!.ParticipantId)
                playerMinions.Add(frame.ParticipantFrames.X9.MinionsKilled +
                                  frame.ParticipantFrames.X1.JungleMinionsKilled);
            if (summonerId == frame.ParticipantFrames.X10!.ParticipantId)
                playerMinions.Add(frame.ParticipantFrames.X10.MinionsKilled +
                                  frame.ParticipantFrames.X1.JungleMinionsKilled);
        }

        var playerMinionsSeries = new LineSeries<int>
        {
            Values = playerMinions,
            Stroke = new SolidColorPaint(new SKColor(41, 128, 185)),
            GeometrySize = 2
        };
        MinionsChart.Series = new List<ISeries> { playerMinionsSeries };


        var playerXp = new ObservableCollection<int>();
        foreach (var frame in timeline.Info.Frames)
        {
            if (summonerId == frame.ParticipantFrames.X1.ParticipantId)
                playerXp.Add(frame.ParticipantFrames.X1.Xp);
            if (summonerId == frame.ParticipantFrames.X2.ParticipantId)
                playerXp.Add(frame.ParticipantFrames.X2.Xp);
            if (summonerId == frame.ParticipantFrames.X3.ParticipantId)
                playerXp.Add(frame.ParticipantFrames.X3.Xp);
            if (summonerId == frame.ParticipantFrames.X4.ParticipantId)
                playerXp.Add(frame.ParticipantFrames.X4.Xp);
            if (summonerId == frame.ParticipantFrames.X5.ParticipantId)
                playerXp.Add(frame.ParticipantFrames.X5.Xp);
            if (summonerId == frame.ParticipantFrames.X6.ParticipantId)
                playerXp.Add(frame.ParticipantFrames.X6.Xp);
            if (summonerId == frame.ParticipantFrames.X7.ParticipantId)
                playerXp.Add(frame.ParticipantFrames.X7.Xp);
            if (summonerId == frame.ParticipantFrames.X8.ParticipantId)
                playerXp.Add(frame.ParticipantFrames.X8.Xp);
            if (summonerId == frame.ParticipantFrames.X9!.ParticipantId)
                playerXp.Add(frame.ParticipantFrames.X9.Xp);
            if (summonerId == frame.ParticipantFrames.X10!.ParticipantId)
                playerXp.Add(frame.ParticipantFrames.X10.Xp);
        }

        var playerXpSeries = new LineSeries<int>
        {
            Values = playerXp,
            Stroke = new SolidColorPaint(new SKColor(41, 128, 185)),
            GeometrySize = 2
        };
        XpChart.Series = new List<ISeries> { playerXpSeries };
    }

    private void SetProfilePicture()
    {
        var source =
            $@"C:\\Users\\alcam\\OneDrive\\Developpement\\nexus-client\\NexusClient\\NexusClient\\Assets\\loldata\\14.1.1\\img\\profileicon\\{Api.SummonerV4().GetBySummonerName(SummonerPlatformRoute, LolSummoner.Name)!.ProfileIconId}.png";
        profilePicture = GetImage(source, 10, 60);
    }

    private void GoldsChartButton_OnClick(object sender, RoutedEventArgs e)
    {
        MinionsChartButton.IsChecked = false;
        XpChartButton.IsChecked = false;
        GoldChart.Visibility = Visibility.Visible;
        XpChart.Visibility = Visibility.Collapsed;
        MinionsChart.Visibility = Visibility.Collapsed;
    }

    private void MinionsChartButton_OnClick(object sender, RoutedEventArgs e)
    {
        GoldsChartButton.IsChecked = false;
        XpChartButton.IsChecked = false;
        GoldChart.Visibility = Visibility.Collapsed;
        XpChart.Visibility = Visibility.Collapsed;
        MinionsChart.Visibility = Visibility.Visible;
    }

    private void XpChartButton_OnClick(object sender, RoutedEventArgs e)
    {
        GoldsChartButton.IsChecked = false;
        MinionsChartButton.IsChecked = false;
        GoldChart.Visibility = Visibility.Collapsed;
        MinionsChart.Visibility = Visibility.Collapsed;
        XpChart.Visibility = Visibility.Visible;
    }
}