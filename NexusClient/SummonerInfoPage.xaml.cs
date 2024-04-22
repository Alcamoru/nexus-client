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
    private UtilisMethods Methods { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not List<object> parameters) return;
        Api = (RiotGamesApi)parameters.ElementAt(0);
        LolSummoner = (Summoner)parameters.ElementAt(1);
        SummonerRegionalRoute = (RegionalRoute)parameters.ElementAt(2);
        SummonerPlatformRoute = (PlatformRoute)parameters.ElementAt(3);
        Methods = new UtilisMethods(Api, LolSummoner, SummonerRegionalRoute, SummonerPlatformRoute);
        SetLastMatches();
        SetLeaderBoardGrid();
        SetBestChampions();
    }

    /// <summary>
    ///     Retrieves the last n matches of a summoner.
    /// </summary>
    /// <returns>A list of Match objects representing the last matches of the summoner.</returns>
    private List<Match> GetLastMatches()
    {
        var matches = new List<Match>();
        var matchListIds = Api.MatchV5().GetMatchIdsByPUUID(SummonerRegionalRoute, LolSummoner.Puuid, 3);
        foreach (var matchListId in matchListIds)
            matches.Add(Api.MatchV5().GetMatch(SummonerRegionalRoute, matchListId));
        return matches;
    }


    /// <summary>
    ///     Sets the leaderboard grid with the top 3 players in the league.
    /// </summary>
    private void SetLeaderBoardGrid()
    {
        // TODO: Re-understand and optimize this
        var bestPlayersList = Api.LeagueV4().GetChallengerLeague(SummonerPlatformRoute, QueueType.RANKED_SOLO_5x5)
            .Entries;
        if (bestPlayersList.Length < 3)
            bestPlayersList = Api.LeagueV4().GetChallengerLeague(SummonerPlatformRoute, QueueType.RANKED_SOLO_5x5)
                .Entries;
        if (bestPlayersList.Length < 3)
            bestPlayersList = Api.LeagueV4().GetMasterLeague(SummonerPlatformRoute, QueueType.RANKED_SOLO_5x5)
                .Entries;
        var leagueItemsSorted = bestPlayersList.OrderByDescending(item => item.LeaguePoints).ToArray();

        var firstPlayer = leagueItemsSorted[0];
        var secondPlayer = leagueItemsSorted[1];
        var thirdPlayer = leagueItemsSorted[2];


        var firstGrid = new Grid
        {
            Padding = new Thickness(10),
            Margin = new Thickness(20, 20, 0, 20),
            CornerRadius = new CornerRadius(12),
            Background = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96))
        };

        // 2 rows definition
        for (var i = 0; i < 2; i++)
            firstGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // 3 columns definition
        for (var i = 0; i < 3; i++)
            firstGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


        var rankingViewbox = new Viewbox
        {
            Child = new Border
            {
                Padding = new Thickness(8),
                CornerRadius = new CornerRadius(8),
                Width = 50,
                Height = 50,
                Background = new SolidColorBrush(Color.FromArgb(255, 243, 156, 18)),
                Child = Methods.SetText("1er", 14, Colors.White, stretch: Stretch.None)
            }
        };
        Grid.SetRow(rankingViewbox, 0);
        firstGrid.Children.Add(rankingViewbox);


        var firstProfileIconImage = Methods.GetProfileIcon(firstPlayer.SummonerId, 10, 60);
        var summonerNameTextBlock = Methods.SetText(Methods.GetSummonerName(firstPlayer.SummonerId), 14, Colors.White);

        var firstInfosViewbox = new Viewbox
        {
            Child = new StackPanel
            {
                Children = { firstProfileIconImage, summonerNameTextBlock }
            }
        };


        Grid.SetRow(firstInfosViewbox, 0);
        Grid.SetColumn(firstInfosViewbox, 1);
        firstGrid.Children.Add(firstInfosViewbox);

        var emblemViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Vertical,
                Children =
                {
                    Methods.GetImage(
                        @"C:\Users\alcam\OneDrive\\Developpement\nexus-client\NexusClient\NexusClient\Assets\emblems\Rank=Challenger.png",
                        0, 70),
                    Methods.SetText($"{firstPlayer.LeaguePoints} LP", 14, Colors.White)
                }
            }
        };

        Grid.SetRow(emblemViewBox, 0);
        Grid.SetColumn(emblemViewBox, 2);
        firstGrid.Children.Add(emblemViewBox);


        var totalGamesRectangle = new Rectangle
        {
            Width = 150,
            Fill = new SolidColorBrush(Colors.White),
            Height = 7
        };

        var gamesWonGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        gamesWonGrid.Children.Add(new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = totalGamesRectangle,
            CornerRadius = new CornerRadius(3)
        });
        gamesWonGrid.Children.Add(new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new Rectangle
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = firstPlayer.Wins / (float)(firstPlayer.Wins + firstPlayer.Losses) * 150,
                Fill = new SolidColorBrush(Color.FromArgb(255, 155, 89, 182)),
                Height = 7
            },
            CornerRadius = new CornerRadius(3)
        });


        var winRateTextBlock = Methods.SetText(
            $"{Math.Round(firstPlayer.Wins / (float)(firstPlayer.Wins + firstPlayer.Losses) * 100)} %",
            14, Colors.White);
        winRateTextBlock.Margin = new Thickness(10);

        var gamesPlayedTextBlock = Methods.SetText($"{firstPlayer.Wins + firstPlayer.Losses} games",
            14, Colors.White);
        gamesPlayedTextBlock.Margin = new Thickness(10);

        var winRateStackPanelViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                Children = { winRateTextBlock, gamesWonGrid, gamesPlayedTextBlock }
            }
        };

        Grid.SetRow(winRateStackPanelViewBox, 1);
        Grid.SetColumn(winRateStackPanelViewBox, 0);
        Grid.SetColumnSpan(winRateStackPanelViewBox, 3);
        firstGrid.Children.Add(winRateStackPanelViewBox);

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

        for (var i = 0; i < 5; i++)
            secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var secondRankingViewbox = new Viewbox
        {
            Child = new Border
            {
                Padding = new Thickness(8),
                CornerRadius = new CornerRadius(8),
                Width = 50,
                Height = 50,
                Background = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185)),
                Child = Methods.SetText("2nd",
                    14, Colors.White)
            }
        };

        Grid.SetRow(secondRankingViewbox, 0);
        secondGrid.Children.Add(secondRankingViewbox);

        var secondProfileIconImage = Methods.GetProfileIcon(secondPlayer.SummonerId, 10, 40);
        var secondSummonerNameTextBlock = Methods.SetText(Methods.GetSummonerName(secondPlayer.SummonerId),
            14, Colors.White);

        var secondInfosViewbox = new Viewbox
        {
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Vertical,
                Children = { secondProfileIconImage, secondSummonerNameTextBlock }
            },
            Margin = new Thickness(7)
        };

        Grid.SetRow(secondInfosViewbox, 0);
        Grid.SetColumn(secondInfosViewbox, 1);
        secondGrid.Children.Add(secondInfosViewbox);


        var source =
            @"C:\Users\alcam\OneDrive\Developpement\nexus-client\NexusClient\NexusClient\Assets\emblems\Rank=Challenger.png";


        var secondEmblemViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Vertical,
                Children =
                {
                    Methods.GetImage(source, 0, 40), Methods.SetText($"{secondPlayer.LeaguePoints} LP",
                        14, Colors.White)
                }
            }
        };

        Grid.SetRow(secondEmblemViewBox, 0);
        Grid.SetColumn(secondEmblemViewBox, 2);
        secondGrid.Children.Add(secondEmblemViewBox);


        var secondGamesWonGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        secondGamesWonGrid.Children.Add(new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = new Rectangle
            {
                Width = 100,
                Fill = new SolidColorBrush(Colors.White),
                Height = 7
            },
            CornerRadius = new CornerRadius(3)
        });
        secondGamesWonGrid.Children.Add(new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new Rectangle
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = firstPlayer.Wins / (float)(firstPlayer.Wins + firstPlayer.Losses) * 100,
                Fill = new SolidColorBrush(Color.FromArgb(255, 155, 89, 182)),
                Height = 7
            },
            CornerRadius = new CornerRadius(3)
        });

        var secondWinRateTextBlock = Methods.SetText(
            $"{Math.Round(secondPlayer.Wins / (float)(secondPlayer.Wins + secondPlayer.Losses) * 100)} %",
            14, Colors.White);
        secondWinRateTextBlock.Margin = new Thickness(10);

        var secondGamesPlayedTextBlock = Methods.SetText($"{firstPlayer.Wins + firstPlayer.Losses} games",
            14, Colors.White);
        secondGamesPlayedTextBlock.Margin = new Thickness(10);

        var secondWinRateViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                Children = { secondWinRateTextBlock, secondGamesWonGrid, secondGamesPlayedTextBlock }
            }
        };

        Grid.SetColumn(secondWinRateViewBox, 3);
        Grid.SetColumnSpan(secondWinRateViewBox, 2);
        secondGrid.Children.Add(secondWinRateViewBox);

        Grid.SetRow(secondGrid, 0);
        Grid.SetColumn(secondGrid, 1);
        LeaderBoardGrid.Children.Add(secondGrid);


        var thirdGrid = new Grid
        {
            Padding = new Thickness(10),
            Margin = new Thickness(20, 10, 20, 20),
            CornerRadius = new CornerRadius(12),
            Background = new SolidColorBrush(Color.FromArgb(255, 241, 196, 15))
        };

        for (var i = 0; i < 5; i++)
            thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var thirdRankingViewbox = new Viewbox
        {
            Child = new Border
            {
                Padding = new Thickness(8),
                CornerRadius = new CornerRadius(8),
                Width = 50,
                Height = 50,
                Background = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185)),
                Child = Methods.SetText("3rd",
                    14, Colors.White)
            }
        };


        Grid.SetRow(thirdRankingViewbox, 0);
        thirdGrid.Children.Add(thirdRankingViewbox);


        var thirdProfileIconImage = Methods.GetProfileIcon(firstPlayer.SummonerId, 10, 40);

        var thirdSummonerNameTextBlock = Methods.SetText(Methods.GetSummonerName(thirdPlayer.SummonerId),
            14, Colors.White);

        var thirdLeaderBoardViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                Margin = new Thickness(7),
                Children = { thirdProfileIconImage, thirdSummonerNameTextBlock }
            }
        };

        Grid.SetRow(thirdLeaderBoardViewBox, 0);
        Grid.SetColumn(thirdLeaderBoardViewBox, 1);
        thirdGrid.Children.Add(thirdLeaderBoardViewBox);

        source =
            @"C:\Users\alcam\OneDrive\Developpement\nexus-client\NexusClient\NexusClient\Assets\emblems\Rank=Challenger.png";

        var thirdEmblemViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Vertical,
                Children =
                {
                    Methods.GetImage(source, 0, 40), Methods.SetText($"{thirdPlayer.LeaguePoints} LP",
                        14, Colors.White)
                }
            }
        };

        Grid.SetRow(thirdEmblemViewBox, 0);
        Grid.SetColumn(thirdEmblemViewBox, 2);
        thirdGrid.Children.Add(thirdEmblemViewBox);

        var thirdGamesWonGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        thirdGamesWonGrid.Children.Add(new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = new Rectangle
            {
                Width = 100,
                Fill = new SolidColorBrush(Colors.White),
                Height = 7
            },
            CornerRadius = new CornerRadius(3)
        });
        thirdGamesWonGrid.Children.Add(new Border
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new Rectangle
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = firstPlayer.Wins / (float)(firstPlayer.Wins + firstPlayer.Losses) * 100,
                Fill = new SolidColorBrush(Color.FromArgb(255, 155, 89, 182)),
                Height = 7
            },
            CornerRadius = new CornerRadius(3)
        });

        var thirdWinRateTextBlock = Methods.SetText($"{thirdPlayer.LeaguePoints} LP",
            14, Colors.White);
        thirdWinRateTextBlock.Margin = new Thickness(10);

        var thirdGamesPlayedTextBlock = Methods.SetText($"{firstPlayer.Wins + firstPlayer.Losses} games",
            14, Colors.White);
        thirdGamesPlayedTextBlock.Margin = new Thickness(10);

        var thirdWinRateViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Horizontal,
                Children = { thirdWinRateTextBlock, thirdGamesWonGrid, thirdGamesPlayedTextBlock }
            }
        };

        Grid.SetColumn(thirdWinRateViewBox, 3);
        Grid.SetColumnSpan(thirdWinRateViewBox, 2);
        thirdGrid.Children.Add(thirdWinRateViewBox);

        Grid.SetRow(thirdGrid, 1);
        Grid.SetColumn(thirdGrid, 1);
        LeaderBoardGrid.Children.Add(thirdGrid);
    }


    /// <summary>
    ///     Sets the last matches data in the UI.
    ///     This method retrieves the last matches data using the GetLastMatches method
    ///     and sets it in a Grid control in the UI. It assigns event handlers to the
    ///     Grid control for handling pointer pressed, pointer entered, and pointer exited
    ///     events. It populates the Grid control with data such as match information,
    ///     champion details, game duration, roles, kills, assists, vision score, etc.
    /// </summary>
    /// <returns>None</returns>
    private void SetLastMatches()
    {
        MatchListGrid.Children.Clear();

        var matches = GetLastMatches();
        var i = 0;
        foreach (var match in matches)
        {
            var matchGrid = new Grid
            {
                Padding = new Thickness(10),
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

            var matchStackPanel = new StackPanel
            {
                Margin = new Thickness(10)
            };

            foreach (var participant in match.Info.Participants)
                if (participant.SummonerId == LolSummoner.Id)
                {
                    if (participant.Win)
                        matchGrid.Background = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185));
                    else
                        matchGrid.Background = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6));

                    var source =
                        $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/champion/{participant.ChampionName}.png";
                    Debug.WriteLine(source);
                    var championIcon = Methods.GetImage(source, 10, 50);

                    championIcon.Margin = new Thickness(10, 0, 0, 0);

                    var champIconViewBox = new Viewbox
                    {
                        Child = championIcon
                    };

                    Grid.SetColumn(champIconViewBox, 0);
                    Grid.SetRow(champIconViewBox, 0);
                    Grid.SetColumnSpan(champIconViewBox, 2);
                    matchGrid.Children.Add(champIconViewBox);

                    var titleChampionTextBlock = Methods.SetText(participant.ChampionName,
                        30, Colors.White);


                    var gameTimeStampDuration = DateTime.Now -
                                                DateTimeOffset.FromUnixTimeMilliseconds(
                                                    // ReSharper disable once PossibleInvalidOperationException
                                                    (long)match.Info.GameEndTimestamp);

                    var matchWasChampionString = "Il y a \n";
                    if (gameTimeStampDuration.Days != 0)
                        matchWasChampionString += $"{gameTimeStampDuration.Days} jours ";
                    if (gameTimeStampDuration.Hours != 0)
                        matchWasChampionString += $"{gameTimeStampDuration.Hours} heures";


                    var matchWasChampionTextBlock = Methods.SetText(matchWasChampionString,
                        14, Colors.White, stretch: Stretch.Uniform);


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

                    source = $"ms-appx:///Assets/media/roles-icons/{participant.TeamPosition}.png";
                    Debug.WriteLine(source);

                    var roleLogo = Methods.GetImage(source, 0, 40);

                    Grid.SetRow(roleLogo, 0);
                    Grid.SetColumn(roleLogo, 4);
                    Grid.SetColumnSpan(roleLogo, 2);
                    matchGrid.Children.Add(roleLogo);

                    var kdaChampionTextBlock = Methods.SetText(
                        $"{participant.Kills} | {participant.Deaths} | {participant.Assists}",
                        14, Colors.White, stretch: Stretch.Uniform);
                    kdaChampionTextBlock.MaxHeight = 30;
                    kdaChampionTextBlock.MaxWidth = 60;

                    Grid.SetColumn(kdaChampionTextBlock, 0);
                    Grid.SetRow(kdaChampionTextBlock, 1);
                    Grid.SetColumnSpan(kdaChampionTextBlock, 2);

                    matchGrid.Children.Add(kdaChampionTextBlock);

                    var teamKills = 1;
                    foreach (var team in match.Info.Teams)
                        if (team.TeamId == participant.TeamId)
                            teamKills = team.Objectives.Champion.Kills;

                    var kpChampionTextBlock = Methods.SetText(
                        $"{Math.Round((float)(participant.Kills + participant.Assists) / teamKills * 100)}",
                        14, Colors.White, stretch: Stretch.Uniform);
                    kpChampionTextBlock.MaxHeight = 30;
                    kpChampionTextBlock.MaxWidth = 60;

                    Grid.SetColumn(kpChampionTextBlock, 2);
                    Grid.SetRow(kpChampionTextBlock, 1);
                    Grid.SetColumnSpan(kpChampionTextBlock, 2);
                    matchGrid.Children.Add(kpChampionTextBlock);

                    var csChampionTextBlock = Methods.SetText(
                        $"{participant.TotalMinionsKilled + participant.TotalAllyJungleMinionsKilled + participant.TotalEnemyJungleMinionsKilled} cs",
                        14, Colors.White, stretch: Stretch.Uniform);
                    csChampionTextBlock.MaxHeight = 30;
                    csChampionTextBlock.MaxWidth = 60;

                    Grid.SetColumn(csChampionTextBlock, 4);
                    Grid.SetRow(csChampionTextBlock, 1);
                    Grid.SetColumnSpan(csChampionTextBlock, 2);
                    matchGrid.Children.Add(csChampionTextBlock);


                    var summonersStackPanel = new StackPanel
                    {
                        Margin = new Thickness(10)
                    };

                    var visionChampionTextBlock = Methods.SetText($"{participant.VisionScore} vision",
                        14, Colors.White);

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

                    source =
                        $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/spell/{sumsCorrespondences[participant.Summoner1Id]}.png";
                    var firstSummonerSpellImage = Methods.GetImage(source);

                    firstSummonerSpellImage.CornerRadius = new CornerRadius(7, 7, 0, 0);


                    Grid.SetColumn(firstSummonerSpellImage, 0);
                    Grid.SetRow(firstSummonerSpellImage, 0);
                    summonerChampionGrid.Children.Add(firstSummonerSpellImage);

                    source =
                        $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/spell/{sumsCorrespondences[participant.Summoner2Id]}.png";
                    var secondSummonerSpellImage = Methods.GetImage(source);
                    secondSummonerSpellImage.CornerRadius = new CornerRadius(0, 0, 7, 7);


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


                    var mainRuneUrl =
                        $"https://ddragon.leagueoflegends.com/cdn/img/{firstPerkIcon}";
                    Debug.WriteLine(mainRuneUrl);

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

                    var matchDurationChampionTextBlock = Methods.SetText(
                        $"{gameDuration.Minutes} minutes\n{gameDuration.Seconds} secondes",
                        14, Colors.White);
                    matchDurationChampionTextBlock.Margin = new Thickness(8);

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

                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item0}.png";
                    Debug.WriteLine(source);
                    var itemImage0 = Methods.GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item1}.png";
                    var itemImage1 = Methods.GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item2}.png";
                    var itemImage2 = Methods.GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item3}.png";
                    var itemImage3 = Methods.GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item4}.png";
                    var itemImage4 = Methods.GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item5}.png";
                    var itemImage5 = Methods.GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item6}.png";
                    var itemImage6 = Methods.GetImage(source, 3);

                    Grid.SetColumn(itemImage0, 0);
                    Grid.SetRow(itemImage0, 0);
                    itemsChampionGrid.Children.Add(itemImage0);
                    Grid.SetColumn(itemImage1, 1);
                    Grid.SetRow(itemImage0, 0);
                    itemsChampionGrid.Children.Add(itemImage1);
                    Grid.SetColumn(itemImage2, 2);
                    Grid.SetRow(itemImage2, 0);
                    itemsChampionGrid.Children.Add(itemImage2);
                    Grid.SetColumn(itemImage3, 3);
                    Grid.SetRow(itemImage3, 0);
                    itemsChampionGrid.Children.Add(itemImage3);
                    Grid.SetColumn(itemImage4, 0);
                    Grid.SetRow(itemImage4, 1);
                    itemsChampionGrid.Children.Add(itemImage4);
                    Grid.SetColumn(itemImage5, 1);
                    Grid.SetRow(itemImage5, 1);
                    itemsChampionGrid.Children.Add(itemImage5);
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
                }

            var matchName = Methods.SetText(match.Info.GameMode.ToString(), 20, Color.FromArgb(255, 52, 73, 94));
            matchStackPanel.Children.Add(matchName);
            matchStackPanel.Children.Add(matchGrid);
            var matchStackPanelViewbox = new Viewbox
            {
                Child = matchStackPanel
            };

            Grid.SetColumn(matchStackPanelViewbox, i);
            MatchListGrid.Children.Add(matchStackPanelViewbox);

            i++;
        }
    }


    /// <summary>
    ///     Sets the best champions in the application.
    /// </summary>
    /// <returns>None</returns>
    private void SetBestChampions()
    {
        BestChampionsContentGrid.Children.Clear();

        var bestChampsjson =
            File.ReadAllText(
                @"C:\Users\alcam\OneDrive\Developpement\nexus-client\NexusClient\NexusClient\bestChampions.json");
        var bestChamps = JsonConvert.DeserializeObject<BestChampionsClass.Root>(bestChampsjson);


        var row = 0;
        foreach (var bestChamp in bestChamps.stats)
        {
            var bestChampionStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10)
            };

            var source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/champion/{bestChamp.name}.png";
            var championIcon = Methods.GetImage(source, 10, 60);
            championIcon.HorizontalAlignment = HorizontalAlignment.Left;
            championIcon.VerticalAlignment = VerticalAlignment.Center;
            championIcon.Margin = new Thickness(10);


            bestChampionStackPanel.Children.Add(championIcon);

            var championNameTextBlock = Methods.SetText($"{bestChamp.name}",
                20, Colors.White);
            championNameTextBlock.Margin = new Thickness(10);

            bestChampionStackPanel.Children.Add(championNameTextBlock);

            var championWrTextBlock = Methods.SetText($"{bestChamp.winrate} %",
                20, Colors.White);
            championWrTextBlock.Margin = new Thickness(10);

            bestChampionStackPanel.Children.Add(championWrTextBlock);

            Grid.SetRow(bestChampionStackPanel, row);
            BestChampionsContentGrid.Children.Add(bestChampionStackPanel);

            row += 1;
        }
    }

    private async void Match_OnPointerPressed(object sender, PointerRoutedEventArgs e)
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
            var matchProgressRing = new ProgressRing
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidColorBrush(Colors.White)
            };

            var matchRectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(150, 52, 73, 94))
            };
            Grid.SetColumn(matchRectangle, 0);
            Grid.SetColumnSpan(matchRectangle, 6);
            Grid.SetRow(matchRectangle, 0);
            Grid.SetRowSpan(matchRectangle, 3);

            matchGrid!.Children.Add(matchRectangle);

            Grid.SetColumn(matchProgressRing, 0);
            Grid.SetColumnSpan(matchProgressRing, 6);
            Grid.SetRow(matchProgressRing, 0);
            Grid.SetRowSpan(matchProgressRing, 3);

            matchGrid!.Children.Add(matchProgressRing);
            await Task.Run(() => { Thread.Sleep(1); });


            var parameters = new List<object>
            {
                Api,
                matchGrid.Tag,
                LolSummoner,
                SummonerRegionalRoute,
                SummonerPlatformRoute
            };
            Frame.Navigate(typeof(MatchDetail), parameters, new DrillInNavigationTransitionInfo());
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
        // var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        // localSettings.Values.Clear();
        Frame.GoBack(new DrillInNavigationTransitionInfo());
    }
}