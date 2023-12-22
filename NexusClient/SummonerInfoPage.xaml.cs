using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Microsoft.UI.Xaml.Shapes;
using Newtonsoft.Json;

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
        SetLeaderBoardGrid();
    }

    private List<Match> GetLastMatches()
    {
        var matches = new List<Match>();
        var matchListIds = Api.MatchV5().GetMatchIdsByPUUID(SummonerRegionalRoute, LolSummoner.Puuid, 3);
        foreach (var matchListId in matchListIds)
            matches.Add(Api.MatchV5().GetMatch(SummonerRegionalRoute, matchListId));

        return matches;
    }

    private void SetLeaderBoardGrid()
    {
        var entries = Api.LeagueV4().GetChallengerLeague(SummonerPlatformRoute, QueueType.RANKED_SOLO_5x5).Entries;
        var leagueItemsSorted = entries.OrderByDescending(item => item.LeaguePoints).ToArray();

        var first = leagueItemsSorted[0];
        var second = leagueItemsSorted[1];
        var third = leagueItemsSorted[2];

        Debug.WriteLine(leagueItemsSorted[0]);

        var firstGrid = new Grid
        {
            Padding = new Thickness(10),
            Margin = new Thickness(20, 20, 0, 20),
            CornerRadius = new CornerRadius(12),
            Background = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96))
        };
        firstGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        firstGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        firstGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        firstGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        firstGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var iconBorder = new Border
        {
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(8),
            Width = 50,
            Height = 50,
            Background = new SolidColorBrush(Color.FromArgb(255, 243, 156, 18))
        };

        var firstTextBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = "1er",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };

        iconBorder.Child = firstTextBlock;

        Grid.SetRow(iconBorder, 0);
        firstGrid.Children.Add(iconBorder);


        var leaderBoardStackPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Vertical
        };

        Viewbox leaderboardViewBox = new Viewbox()
        {
            Child = leaderBoardStackPanel
        };

        var profileIconImage = new Image
        {
            Width = 60,
            Source = new BitmapImage(new Uri(
                $@"C:\\Users\\alcam\\OneDrive\\Documents\\Developpement\\nexus-client\\NexusClient\\NexusClient\\Assets\\loldata\\13.24.1\\img\\profileicon\\{Api.SummonerV4().GetBySummonerName(SummonerPlatformRoute, first.SummonerName)!.ProfileIconId}.png"))
        };

        Border profileIconBorder = new Border()
        {
            CornerRadius = new CornerRadius(10),
            Child = profileIconImage
        };

        var summonerNameTextBlock = new TextBlock
        {
            Text = $"{first.SummonerName}",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };


        leaderBoardStackPanel.Children.Add(profileIconBorder);
        leaderBoardStackPanel.Children.Add(summonerNameTextBlock);

        Grid.SetRow(leaderboardViewBox, 0);
        Grid.SetColumn(leaderboardViewBox, 1);
        firstGrid.Children.Add(leaderboardViewBox);


        var emblemStackPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Vertical
        };

        Viewbox emblemStackPanelViewBox = new Viewbox()
        {
            Child = emblemStackPanel
        };

        var emblemIcon = new Image
        {
            Width = 70,
            Source = new BitmapImage(new Uri(
                @"C:\Users\alcam\OneDrive\Documents\Developpement\nexus-client\NexusClient\NexusClient\Assets\emblems\Rank=Challenger.png"))
        };

        var lpTextBlock = new TextBlock
        {
            Text = $"{first.LeaguePoints} LP",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };


        emblemStackPanel.Children.Add(emblemIcon);
        emblemStackPanel.Children.Add(lpTextBlock);

        Grid.SetRow(emblemStackPanelViewBox, 0);
        Grid.SetColumn(emblemStackPanelViewBox, 2);
        firstGrid.Children.Add(emblemStackPanelViewBox);


        var totalGamesRectangle = new Rectangle()
        {
            Width = 150,
            Fill = new SolidColorBrush(Colors.White),
            Height = 7
        };

        var totalGamesBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = totalGamesRectangle,
            CornerRadius = new CornerRadius(3)
        };


        var width = first.Wins / (float)(first.Wins + first.Losses) * 150;

        var gamesWonRectangle = new Rectangle
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = width,
            Fill = new SolidColorBrush(Color.FromArgb(255, 155, 89, 182)),
            Height = 7
        };

        var gamesWonBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = gamesWonRectangle,
            CornerRadius = new CornerRadius(3)
        };

        var gamesWonGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        gamesWonGrid.Children.Add(totalGamesBorder);
        gamesWonGrid.Children.Add(gamesWonBorder);

        Grid.SetRow(gamesWonGrid, 1);
        Grid.SetColumn(gamesWonGrid, 0);
        Grid.SetColumnSpan(gamesWonGrid, 3);
        firstGrid.Children.Add(gamesWonGrid);

        Grid.SetRow(firstGrid, 0);
        Grid.SetRowSpan(firstGrid, 2);
        Grid.SetColumn(firstGrid, 0);
        LeaderBoardGrid.Children.Add(firstGrid);

        var secondGrid = new Grid
        {
            Padding = new Thickness(10),
            Margin = new Thickness(20, 20, 20, 10),
            CornerRadius = new CornerRadius(12),
            Background = new SolidColorBrush(Color.FromArgb(255, 241, 196, 15))
        };

        secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var secondIconBorder = new Border
        {
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(8),
            Width = 50,
            Height = 50,
            Background = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185))
        };

        var secondTextBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = "2nd",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };

        secondIconBorder.Child = secondTextBlock;

        Grid.SetRow(secondIconBorder, 0);
        secondGrid.Children.Add(secondIconBorder);


        var secondLeaderBoardStackPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Vertical
        };

        Viewbox secondLeaderBoardStackPanelViewBox = new Viewbox()
        {
            Child = secondLeaderBoardStackPanel
        };

        var secondProfileIconImage = new Image
        {
            Width = 40,
            Source = new BitmapImage(new Uri(
                $@"C:\\Users\\alcam\\OneDrive\\Documents\\Developpement\\nexus-client\\NexusClient\\NexusClient\\Assets\\loldata\\13.24.1\\img\\profileicon\\{Api.SummonerV4().GetBySummonerName(SummonerPlatformRoute, second.SummonerName)!.ProfileIconId}.png"))
        };

        var secondSummonerNameTextBlock = new TextBlock
        {
            Text = $"{second.SummonerName}",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };


        secondLeaderBoardStackPanel.Children.Add(secondProfileIconImage);
        secondLeaderBoardStackPanel.Children.Add(secondSummonerNameTextBlock);

        Grid.SetRow(secondLeaderBoardStackPanelViewBox, 0);
        Grid.SetColumn(secondLeaderBoardStackPanelViewBox, 1);
        secondGrid.Children.Add(secondLeaderBoardStackPanelViewBox);


        var secondEmblemStackPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Vertical
        };

        Viewbox secondEmblemStackPanelViewBox = new Viewbox()
        {
            Child = secondEmblemStackPanel
        };

        var secondEmblemIcon = new Image
        {
            Width = 40,
            Source = new BitmapImage(new Uri(
                @"C:\Users\alcam\OneDrive\Documents\Developpement\nexus-client\NexusClient\NexusClient\Assets\emblems\Rank=Challenger.png"))
        };

        var secondLpTextBlock = new TextBlock
        {
            Text = $"{second.LeaguePoints} LP",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };


        secondEmblemStackPanel.Children.Add(secondEmblemIcon);
        secondEmblemStackPanel.Children.Add(secondLpTextBlock);

        Grid.SetRow(secondEmblemStackPanelViewBox, 0);
        Grid.SetColumn(secondEmblemStackPanelViewBox, 2);
        secondGrid.Children.Add(secondEmblemStackPanelViewBox);


        var secondTotalGamesRectangle = new Rectangle()
        {
            Width = 100,
            Fill = new SolidColorBrush(Colors.White),
            Height = 7
        };

        var secondGamesBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = secondTotalGamesRectangle,
            CornerRadius = new CornerRadius(3)
        };


        var secondWidth = first.Wins / (float)(first.Wins + first.Losses) * 100;

        var secondGamesWonRectangle = new Rectangle
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = secondWidth,
            Fill = new SolidColorBrush(Color.FromArgb(255, 155, 89, 182)),
            Height = 7
        };

        var secondGamesWonBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = secondGamesWonRectangle,
            CornerRadius = new CornerRadius(3)
        };

        var secondGamesWonGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        secondGamesWonGrid.Children.Add(secondGamesBorder);
        secondGamesWonGrid.Children.Add(secondGamesWonBorder);

        Grid.SetColumn(secondGamesWonGrid, 3);
        Grid.SetColumnSpan(secondGamesWonGrid, 2);
        secondGrid.Children.Add(secondGamesWonGrid);

        Grid.SetRow(secondGrid, 0);
        Grid.SetColumn(secondGrid, 1);
        LeaderBoardGrid.Children.Add(secondGrid);

        var thirdGrid = new Grid
        {
            Padding = new Thickness(10),
            Margin = new Thickness(20, 20, 20, 10),
            CornerRadius = new CornerRadius(12),
            Background = new SolidColorBrush(Color.FromArgb(255, 241, 196, 15))
        };

        thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var thirdIconBorder = new Border
        {
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(8),
            Width = 50,
            Height = 50,
            Background = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185))
        };

        var thirdTextBlock = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = "3rd",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };

        thirdIconBorder.Child = thirdTextBlock;

        Grid.SetRow(thirdIconBorder, 0);
        thirdGrid.Children.Add(thirdIconBorder);


        var thirdLeaderBoardStackPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Vertical
        };

        Viewbox thirdLeaderBoardStackPanelViewBox = new Viewbox()
        {
            Child = thirdLeaderBoardStackPanel
        };

        var thirdProfileIconImage = new Image
        {
            Width = 40,
            Source = new BitmapImage(new Uri(
                $@"C:\\Users\\alcam\\OneDrive\\Documents\\Developpement\\nexus-client\\NexusClient\\NexusClient\\Assets\\loldata\\13.24.1\\img\\profileicon\\{Api.SummonerV4().GetBySummonerName(SummonerPlatformRoute, third.SummonerName)!.ProfileIconId}.png"))
        };

        var thirdSummonerNameTextBlock = new TextBlock
        {
            Text = $"{third.SummonerName}",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };


        thirdLeaderBoardStackPanel.Children.Add(thirdProfileIconImage);
        thirdLeaderBoardStackPanel.Children.Add(thirdSummonerNameTextBlock);

        Grid.SetRow(thirdLeaderBoardStackPanelViewBox, 0);
        Grid.SetColumn(thirdLeaderBoardStackPanelViewBox, 1);
        thirdGrid.Children.Add(thirdLeaderBoardStackPanelViewBox);


        var thirdEmblemStackPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Orientation = Orientation.Vertical
        };

        Viewbox thirdEmblemStackPanelViewBox = new Viewbox()
        {
            Child = thirdEmblemStackPanel
        };

        var thirdEmblemIcon = new Image
        {
            Width = 40,
            Source = new BitmapImage(new Uri(
                @"C:\Users\alcam\OneDrive\Documents\Developpement\nexus-client\NexusClient\NexusClient\Assets\emblems\Rank=Challenger.png"))
        };

        var thirdLpTextBlock = new TextBlock
        {
            Text = $"{third.LeaguePoints} LP",
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 14,
            TextAlignment = TextAlignment.Center,
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };


        thirdEmblemStackPanel.Children.Add(thirdEmblemIcon);
        thirdEmblemStackPanel.Children.Add(thirdLpTextBlock);

        Grid.SetRow(thirdEmblemStackPanelViewBox, 0);
        Grid.SetColumn(thirdEmblemStackPanelViewBox, 2);
        thirdGrid.Children.Add(thirdEmblemStackPanelViewBox);


        var thirdTotalGamesRectangle = new Rectangle()
        {
            Width = 100,
            Fill = new SolidColorBrush(Colors.White),
            Height = 7
        };

        var thirdGamesBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = thirdTotalGamesRectangle,
            CornerRadius = new CornerRadius(3)
        };


        var thirdWidth = first.Wins / (float)(first.Wins + first.Losses) * 100;

        var thirdGamesWonRectangle = new Rectangle
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = thirdWidth,
            Fill = new SolidColorBrush(Color.FromArgb(255, 155, 89, 182)),
            Height = 7
        };

        var thirdGamesWonBorder = new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = thirdGamesWonRectangle,
            CornerRadius = new CornerRadius(3)
        };

        var thirdGamesWonGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        thirdGamesWonGrid.Children.Add(thirdGamesBorder);
        thirdGamesWonGrid.Children.Add(thirdGamesWonBorder);

        Grid.SetColumn(thirdGamesWonGrid, 3);
        Grid.SetColumnSpan(thirdGamesWonGrid, 2);
        thirdGrid.Children.Add(thirdGamesWonGrid);


        Grid.SetRow(thirdGrid, 1);
        Grid.SetColumn(thirdGrid, 1);
        LeaderBoardGrid.Children.Add(thirdGrid);


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

                    var sumsCorrespondences = new Dictionary<int, string>
                    {
                        { 21, "SummonerBarrier" },
                        { 1, "SummonerBoost" },
                        { 2202, "SummonerCherryFlash" },
                        { 2201, "SummonerCherryHold" },
                        { 14, "SummonerDot" },
                        { 3, "SummonerExhaust" },
                        { 4, "SummonerFlash" },
                        { 6, "SummonerHaste" },
                        { 7, "SummonerHeal" },
                        { 13, "SummonerMana" },
                        { 30, "SummonerPoroRecall" },
                        { 31, "SummonerPoroThrow" },
                        { 11, "SummonerSmite" },
                        { 39, "SummonerSnowURFSnowball_Mark" },
                        { 32, "SummonerSnowball" },
                        { 12, "SummonerTeleport" },
                        { 54, "Summoner_UltBookPlaceholder" },
                        { 55, "Summoner_UltBookSmitePlaceholder" }
                    };


                    var firstSummonerSpellImage = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.24.1/img/spell/{sumsCorrespondences[participant.Summoner1Id]}.png"))
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
                            $"http://ddragon.leagueoflegends.com/cdn/13.24.1/img/spell/{sumsCorrespondences[participant.Summoner2Id]}.png"))
                    };

                    var secondSummonerSpellBorder = new Border
                    {
                        CornerRadius = new CornerRadius(0, 0, 7, 7),
                        Child = secondSummonerSpellImage
                    };

                    Grid.SetColumn(secondSummonerSpellBorder, 0);
                    Grid.SetRow(secondSummonerSpellBorder, 1);
                    summonerChampionGrid.Children.Add(secondSummonerSpellBorder);


                    var perksJson =
                        File.ReadAllText(
                            @"C:\Users\alcam\OneDrive\Documents\Developpement\nexus-client\NexusClient\NexusClient\Assets\loldata\13.24.1\data\fr_FR\runesReforged.json");
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


                    var mainRuneUrl =
                        $"https://ddragon.leagueoflegends.com/cdn/img/{firstPerkIcon}";

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
                            $"https://ddragon.leagueoflegends.com/cdn/img/{secondPerkIcon}"))
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
                        CornerRadius = new CornerRadius(10),
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