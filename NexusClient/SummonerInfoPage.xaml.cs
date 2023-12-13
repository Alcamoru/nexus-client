using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.UI.Xaml.Media.Imaging;
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

    private RiotGamesApi Api { get; set; }

    private Summoner LolSummoner { get; set; }

    private RegionalRoute SummonerRegionalRoute { get; set; }

    private PlatformRoute SummonerPlatformRoute { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not List<object> parameters) return;
        Api = (RiotGamesApi)parameters.ElementAt(0);
        LolSummoner = (Summoner)parameters.ElementAt(1);

        SummonerRegionalRoute = (RegionalRoute)parameters.ElementAt(2);
        SummonerPlatformRoute = (PlatformRoute)parameters.ElementAt(3);
        SetLastMatches();
    }

    private List<Match> GetLastMatches()
    {
        var matches = new List<Match>();
        var matchListIds = Api.MatchV5().GetMatchIdsByPUUID(SummonerRegionalRoute, LolSummoner.Puuid, 3);
        foreach (var matchListId in matchListIds)
            matches.Add(Api.MatchV5().GetMatch(SummonerRegionalRoute, matchListId));

        return matches;
    }

    private void SetLastMatches()
    {
        var matches = GetLastMatches();
        var i = 0;
        foreach (var match in matches)
        {
            var matchGrid = new Grid
            {
                Margin = new Thickness(10),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                CornerRadius = new CornerRadius(10)
            };

            matchGrid.PointerPressed += Match_OnPointerPressed;
            matchGrid.PointerEntered += MatchGridOnPointerEntered;
            matchGrid.PointerExited += MatchGridOnPointerExited;

            matchGrid.Tag = match;

            var col1 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col2 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col3 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col4 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col5 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col6 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };

            matchGrid.ColumnDefinitions.Add(col1);
            matchGrid.ColumnDefinitions.Add(col2);
            matchGrid.ColumnDefinitions.Add(col3);
            matchGrid.ColumnDefinitions.Add(col4);
            matchGrid.ColumnDefinitions.Add(col5);
            matchGrid.ColumnDefinitions.Add(col6);

            var row1 = new RowDefinition { Height = new GridLength(5, GridUnitType.Star) };
            var row2 = new RowDefinition { Height = new GridLength(5, GridUnitType.Star) };
            var row3 = new RowDefinition { Height = new GridLength(7, GridUnitType.Star) };

            matchGrid.RowDefinitions.Add(row1);
            matchGrid.RowDefinitions.Add(row2);
            matchGrid.RowDefinitions.Add(row3);

            foreach (var participant in match.Info.Participants)
                if (participant.SummonerName == LolSummoner.Name)
                {
                    if (participant.Win)
                        matchGrid.Background = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185));
                    else
                        matchGrid.Background = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6));

                    var championIcon = new Image
                    {
                        Width = 50,
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/{participant.ChampionName}.png"))
                    };

                    var summonerIconBorder = new Border
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        CornerRadius = new CornerRadius(10)
                    };
                    summonerIconBorder.Child = championIcon;
                    Grid.SetColumn(summonerIconBorder, 0);
                    Grid.SetRow(summonerIconBorder, 0);
                    Grid.SetColumnSpan(summonerIconBorder, 2);
                    matchGrid.Children.Add(summonerIconBorder);

                    var titleChampionTextBlock = new TextBlock
                    {
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = participant.ChampionName,
                        FontSize = 20,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };


                    var gameTimeStampDuration = DateTime.Now -
                                                DateTimeOffset.FromUnixTimeMilliseconds(
                                                    // ReSharper disable once PossibleInvalidOperationException
                                                    (long)match.Info.GameEndTimestamp);

                    var matchWasChampionString = "Il y a \n";
                    if (gameTimeStampDuration.Days != 0)
                        matchWasChampionString += $"{gameTimeStampDuration.Days} jours ";
                    if (gameTimeStampDuration.Hours != 0)
                        matchWasChampionString += $"{gameTimeStampDuration.Hours} heures";
                    var matchWasChampionTextBlock = new TextBlock
                    {
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = matchWasChampionString,
                        FontSize = 10,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    var titlesChampionStackPanel = new StackPanel
                    {
                        Padding = new Thickness(8),
                        Orientation = Orientation.Vertical
                    };
                    titlesChampionStackPanel.Children.Add(titleChampionTextBlock);
                    titlesChampionStackPanel.Children.Add(matchWasChampionTextBlock);

                    var titleChampionViewBox = new Viewbox
                    {
                        Child = titlesChampionStackPanel,
                        Stretch = Stretch.Uniform
                    };

                    Grid.SetColumn(titleChampionViewBox, 2);
                    Grid.SetRow(titleChampionViewBox, 0);
                    Grid.SetColumnSpan(titleChampionViewBox, 2);

                    matchGrid.Children.Add(titleChampionViewBox);

                    var roleLogo = new Image
                    {
                        Width = 40,
                        Source = new BitmapImage(
                            new Uri($"ms-appx:///Assets/media/roles-icons/{participant.TeamPosition}.png"))
                    };
                    Grid.SetRow(roleLogo, 0);
                    Grid.SetColumn(roleLogo, 4);
                    Grid.SetColumnSpan(roleLogo, 2);
                    matchGrid.Children.Add(roleLogo);

                    var kdaChampionTextBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Text = $"{participant.Kills} | {participant.Deaths} | {participant.Assists}",
                        FontSize = 15,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    var kdaChampionViewbox = new Viewbox
                    {
                        MaxWidth = 60,
                        MaxHeight = 30,
                        Child = kdaChampionTextBlock,
                        Stretch = Stretch.Uniform
                    };

                    Grid.SetColumn(kdaChampionViewbox, 0);
                    Grid.SetRow(kdaChampionViewbox, 1);
                    Grid.SetColumnSpan(kdaChampionViewbox, 2);

                    matchGrid.Children.Add(kdaChampionViewbox);

                    var teamKills = 1;
                    foreach (var team in match.Info.Teams)
                        if (team.TeamId == participant.TeamId)
                            teamKills = team.Objectives.Champion.Kills;


                    var kpChampionTextBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Text = $"{(participant.Kills + participant.Assists) / teamKills}",
                        FontSize = 15,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    var kpChampionViewbox = new Viewbox
                    {
                        MaxWidth = 60,
                        MaxHeight = 30,
                        Child = kpChampionTextBlock,
                        Stretch = Stretch.Uniform
                    };

                    Grid.SetColumn(kpChampionViewbox, 2);
                    Grid.SetRow(kpChampionViewbox, 1);
                    Grid.SetColumnSpan(kpChampionViewbox, 2);
                    matchGrid.Children.Add(kpChampionViewbox);


                    var csChampionTextBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.White),
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Text = $"{participant.TotalMinionsKilled} cs",
                        FontSize = 15,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    var csChampionViewbox = new Viewbox
                    {
                        MaxWidth = 60,
                        MaxHeight = 30,
                        Child = csChampionTextBlock,
                        Stretch = Stretch.Uniform
                    };

                    Grid.SetColumn(csChampionViewbox, 4);
                    Grid.SetRow(csChampionViewbox, 1);
                    Grid.SetColumnSpan(csChampionViewbox, 2);

                    matchGrid.Children.Add(csChampionViewbox);


                    var summonersStackPanel = new StackPanel
                    {
                        Margin = new Thickness(10)
                    };
                    var visionChampionTextBlock = new TextBlock
                    {
                        Foreground = new SolidColorBrush(Colors.White),
                        Padding = new Thickness(8),
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Text = $"{participant.VisionScore} vision",
                        FontSize = 15,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

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
                    var firstSummonerSpellBorder = new Border
                    {
                        CornerRadius = new CornerRadius(7, 7, 0, 0),
                        Child = firstSummonerSpellImage
                    };

                    Grid.SetColumn(firstSummonerSpellBorder, 0);
                    Grid.SetRow(firstSummonerSpellBorder, 0);
                    summonerChampionGrid.Children.Add(firstSummonerSpellBorder);

                    var secondSummonerSpellImage = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.17.1/img/spell/{sumsCorrespondences[participant.Summoner2Id.ToString()]}.png"))
                    };

                    var secondSummonerSpellBorder = new Border
                    {
                        CornerRadius = new CornerRadius(0, 0, 7, 7),
                        Child = secondSummonerSpellImage
                    };

                    Grid.SetColumn(secondSummonerSpellBorder, 0);
                    Grid.SetRow(secondSummonerSpellBorder, 1);
                    summonerChampionGrid.Children.Add(secondSummonerSpellBorder);

                    string mainRuneUrl=
                        $"https://ddragon.leagueoflegends.com/cdn/img/perk-images/Styles/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][0]}/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][1]}/{mainPerksCorrespondences[participant.Perks.Styles[0].Selections[0].Perk.ToString()][1]}.png";

                    var mainRune = new Image
                    {
                        Source = new BitmapImage(new Uri(mainRuneUrl))
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


                    summonersStackPanel.Children.Add(visionChampionTextBlock);
                    summonersStackPanel.Children.Add(summonerChampionGrid);

                    var summonersViewbox = new Viewbox
                    {
                        Child = summonersStackPanel,
                        Stretch = Stretch.Uniform
                    };

                    Grid.SetColumn(summonersViewbox, 0);
                    Grid.SetRow(summonersViewbox, 2);
                    Grid.SetColumnSpan(summonersViewbox, 3);
                    matchGrid.Children.Add(summonersViewbox);


                    var gameDuration = DateTimeOffset.FromUnixTimeMilliseconds((long)match.Info.GameEndTimestamp) -
                                       DateTimeOffset.FromUnixTimeMilliseconds(match.Info.GameStartTimestamp);

                    var itemsChampionStackPanel = new StackPanel();
                    var matchDurationChampionTextBlock = new TextBlock
                    {
                        Foreground = new SolidColorBrush(Colors.White),
                        Padding = new Thickness(8),
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Text = $"{gameDuration.Minutes} minutes\n{gameDuration.Seconds} secondes",
                        FontSize = 15,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    var itemsChampionGrid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    var itemColumn1 = new ColumnDefinition { Width = new GridLength(20, GridUnitType.Pixel) };
                    var itemColumn2 = new ColumnDefinition { Width = new GridLength(20, GridUnitType.Pixel) };
                    var itemColumn3 = new ColumnDefinition { Width = new GridLength(20, GridUnitType.Pixel) };
                    var itemColumn4 = new ColumnDefinition { Width = new GridLength(20, GridUnitType.Pixel) };

                    itemsChampionGrid.ColumnDefinitions.Add(itemColumn1);
                    itemsChampionGrid.ColumnDefinitions.Add(itemColumn2);
                    itemsChampionGrid.ColumnDefinitions.Add(itemColumn3);
                    itemsChampionGrid.ColumnDefinitions.Add(itemColumn4);

                    var itemRow1 = new RowDefinition { Height = new GridLength(20, GridUnitType.Pixel) };
                    var itemRow2 = new RowDefinition { Height = new GridLength(20, GridUnitType.Pixel) };

                    itemsChampionGrid.RowDefinitions.Add(itemRow1);
                    itemsChampionGrid.RowDefinitions.Add(itemRow2);

                    var itemImage0 = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.24.1/img/item/{participant.Item0}.png"))
                    };

                    Grid.SetColumn(itemImage0, 0);
                    Grid.SetRow(itemImage0, 0);
                    itemsChampionGrid.Children.Add(itemImage0);

                    var itemImage1 = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.24.1/img/item/{participant.Item1}.png"))
                    };

                    Grid.SetColumn(itemImage1, 1);
                    Grid.SetRow(itemImage0, 0);
                    itemsChampionGrid.Children.Add(itemImage1);


                    var itemImage2 = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.24.1/img/item/{participant.Item2}.png"))
                    };

                    Grid.SetColumn(itemImage2, 2);
                    Grid.SetRow(itemImage2, 0);
                    itemsChampionGrid.Children.Add(itemImage2);


                    var itemImage3 = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.24.1/img/item/{participant.Item3}.png"))
                    };

                    Grid.SetColumn(itemImage3, 3);
                    Grid.SetRow(itemImage3, 0);
                    itemsChampionGrid.Children.Add(itemImage3);


                    var itemImage4 = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.24.1/img/item/{participant.Item4}.png"))
                    };

                    Grid.SetColumn(itemImage4, 0);
                    Grid.SetRow(itemImage4, 1);
                    itemsChampionGrid.Children.Add(itemImage4);


                    var itemImage5 = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.24.1/img/item/{participant.Item5}.png"))
                    };

                    Grid.SetColumn(itemImage5, 1);
                    Grid.SetRow(itemImage5, 1);
                    itemsChampionGrid.Children.Add(itemImage5);


                    var itemImage6 = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.24.1/img/item/{participant.Item6}.png"))
                    };

                    Grid.SetColumn(itemImage6, 2);
                    Grid.SetRow(itemImage6, 1);
                    itemsChampionGrid.Children.Add(itemImage6);


                    itemsChampionStackPanel.Children.Add(matchDurationChampionTextBlock);
                    itemsChampionStackPanel.Children.Add(itemsChampionGrid);

                    var itemsChampionViewbox = new Viewbox
                    {
                        Child = itemsChampionStackPanel,
                        Stretch = Stretch.Uniform
                    };

                    Grid.SetColumn(itemsChampionViewbox, 3);
                    Grid.SetRow(itemsChampionViewbox, 2);
                    Grid.SetColumnSpan(itemsChampionViewbox, 3);

                    matchGrid.Children.Add(itemsChampionViewbox);


                    Grid.SetColumn(matchGrid, i);
                    MatchListGrid.Children.Add(matchGrid);
                }

            i++;
        }
    }

    private void MatchGridOnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputCursor.CreateFromCoreCursor(new CoreCursor(CoreCursorType.Arrow, 1));
    }

    private void MatchGridOnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputCursor.CreateFromCoreCursor(new CoreCursor(CoreCursorType.Hand, 1));
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(WelcomePage));
    }

    private void Match_OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var originalSource = e.OriginalSource as FrameworkElement;

        Grid matchGrid = null;

        while (originalSource != null)
        {
            if (originalSource is Grid)
            {
                matchGrid = originalSource as Grid;
                break;
            }

            originalSource = originalSource.Parent as FrameworkElement;
        }

        if (matchGrid != null)
        {
            var parameters = new List<object>
            {
                Api,
                matchGrid.Tag,
                LolSummoner,
                SummonerRegionalRoute,
                SummonerPlatformRoute
            };
            Frame.Navigate(typeof(MatchDetail), parameters);
        }
    }
}