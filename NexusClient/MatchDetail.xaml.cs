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
using Microsoft.UI.Xaml.Shapes;
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

    private Summoner LolSummoner { get; set; }

    private Match MatchInfo { get; set; }

    private RegionalRoute SummonerRegionalRoute { get; set; }

    private PlatformRoute SummonerPlatformRoute { get; set; }


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
        var team1 = new List<Participant>();
        var team2 = new List<Participant>();

        foreach (var participant in MatchInfo.Info.Participants)
            if (participant.TeamId == Team.Blue)
                team1.Add(participant);
            else
                team2.Add(participant);

        var row = 0;

        var team1Damages = 0;
        foreach (var participant in team1) team1Damages += participant.TotalDamageDealtToChampions;
        var team2Damages = 0;
        foreach (var participant in team2) team2Damages += participant.TotalDamageDealtToChampions;

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


            var champIconBorder = new Border
            {
                Height = 30,
                Width = 30,
                CornerRadius = new CornerRadius(15)
            };

            var championIcon = new Image
            {
                Source = new BitmapImage(new Uri(
                    $"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/champion/{participant.ChampionName}.png"))
            };

            champIconBorder.Child = championIcon;

            var championIconViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = champIconBorder
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
                Text = participant.ChampionName,
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

            summonersViewbox.Child = summonerChampionGrid;

            Grid.SetColumn(summonersViewbox, 2);
            participantGrid.Children.Add(summonersViewbox);

            var csChampionTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromArgb(255, 52, 73, 94)),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Text = $"{participant.TotalMinionsKilled} CS",
                FontSize = 15,
                FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
            };
            Debug.WriteLine(participant.NeutralMinionsKilled);

            var csChampionViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            csChampionViewbox.Child = csChampionTextBlock;

            Grid.SetColumn(csChampionViewbox, 3);
            participantGrid.Children.Add(csChampionViewbox);

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


            var width = participant.TotalDamageDealtToChampions / (float)team1Damages * 50;

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


            var kdaChampionTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromArgb(255, 52, 73, 94)),
                HorizontalTextAlignment = TextAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Text = $"{participant.Kills} | {participant.Deaths} | {participant.Assists}",
                FontSize = 15,
                FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
            };

            var kdaChampionViewbox = new Viewbox
            {
                Child = kdaChampionTextBlock,
                Stretch = Stretch.Uniform
            };

            Grid.SetColumn(kdaChampionViewbox, 5);
            participantGrid.Children.Add(kdaChampionViewbox);

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


            var champIconBorder = new Border
            {
                Height = 30,
                Width = 30,
                CornerRadius = new CornerRadius(15)
            };

            var championIcon = new Image
            {
                Source = new BitmapImage(new Uri(
                    $"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/champion/{participant.ChampionName}.png"))
            };

            champIconBorder.Child = championIcon;

            var championIconViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = champIconBorder
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
                Text = participant.ChampionName,
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


            var width = participant.TotalDamageDealtToChampions / (float)team2Damages * 50;

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


            var csChampionTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromArgb(255, 52, 73, 94)),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Text = $"{participant.TotalMinionsKilled} CS",
                FontSize = 15,
                FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
            };

            var csChampionViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            csChampionViewbox.Child = csChampionTextBlock;

            Grid.SetColumn(csChampionViewbox, 3);
            participantGrid.Children.Add(csChampionViewbox);


            var kdaChampionTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromArgb(255, 52, 73, 94)),
                HorizontalTextAlignment = TextAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Text = $"{participant.Kills} | {participant.Deaths} | {participant.Assists}",
                FontSize = 15,
                FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
            };

            var kdaChampionViewbox = new Viewbox
            {
                Child = kdaChampionTextBlock,
                Stretch = Stretch.Uniform
            };

            Grid.SetColumn(kdaChampionViewbox, 5);
            participantGrid.Children.Add(kdaChampionViewbox);


            Grid.SetColumn(participantGrid, 2);
            Grid.SetRow(participantGrid, row);
            ParticipantsGrid.Children.Add(participantGrid);
        }
    }


    private void SetMatchTimeline()
    {
        var elementNumber = 0;

        var precedentEventIsPositive = true;


        SetPositiveEvent();
        elementNumber += 1;

        var timeline = Api.MatchV5().GetTimeline(SummonerRegionalRoute, MatchInfo.Metadata.MatchId);
        var summonerParticipantId = 0;
        foreach (var participant in timeline!.Info.Participants!)
        {
            if (participant.Puuid == LolSummoner.Puuid) summonerParticipantId = participant.ParticipantId;
            ;
        }


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

        ;

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
            Orientation = Orientation.Horizontal
        };

        var startIcon = new Image
        {
            Width = 40,
            Source = new BitmapImage(
                new Uri("ms-appx:///Assets/media/bouton-de-lecture-video.png"))
        };

        var startTextBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalTextAlignment = TextAlignment.Center,
            Text = " début du match",
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };

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

                    var eliminatedTextBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = " a eliminé",
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

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
                        Orientation = Orientation.Horizontal
                    };

                    var killerPuuid = "";

                    foreach (var participant in timeline.Info.Participants)
                        if (participant.ParticipantId == e.KillerId)
                            killerPuuid = participant.Puuid;


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
                            $"http://ddragon.leagueoflegends.com/cdn/13.22.1/img/champion/{GetChampionNameByPuuid(LolSummoner.Puuid)}.png")),
                        Width = 40
                    };

                    var eliminatedTextBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = " a eliminé",
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

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
                        Orientation = Orientation.Horizontal
                    };

                    var victimPuuid = "";

                    foreach (var participant in timeline.Info.Participants)
                        if (participant.ParticipantId == e.VictimId)
                            victimPuuid = participant.Puuid;


                    var summonerChampionIcon = new Image
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.22.1/img/champion/{GetChampionNameByPuuid(LolSummoner.Puuid)}.png")),
                        Width = 40
                    };

                    var victimChampionIcon = new Image
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.22.1/img/champion/{GetChampionNameByPuuid(victimPuuid)}.png")),
                        Width = 40
                    };

                    var eliminatedTextBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = " a participé à l'élimination de ",
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

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
    }
}