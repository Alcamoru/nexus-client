using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Camille.Enums;
using Camille.RiotGames;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using static NexusClient.SummonerName;
using static NexusClient.UtilisMethods;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.


namespace NexusClient;

/// <summary>
///     Represents a page that displays information about a summoner.
/// </summary>
public sealed partial class WelcomePage : Page
{
    public WelcomePage()
    {
        InitializeComponent();
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        SetLastMatches();
        SetLeaderBoardGrid();
    }

    /// <summary>
    ///     Retrieves the last n matches of a summoner.
    /// </summary>
    /// <returns>A list of Match objects representing the last matches of the summoner.</returns>
    /// <summary>
    ///     Sets the leaderboard grid with the top 3 players in the league.
    /// </summary>
    private void SetLeaderBoardGrid()
    {
        LeaderBoardGrid.Children.Clear();
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
            Margin = new Thickness(10, 10, 0, 10),
            CornerRadius = new CornerRadius(12),
            Background = AppColors.GreyCool
        };

        // 2 rows definition
        for (var i = 0; i < 2; i++)
            firstGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // 3 columns definition
        for (var i = 0; i < 3; i++)
            firstGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var rankingViewbox = new Viewbox
        {
            Child = new StackPanel
            {
                Margin = new Thickness(15),
                Height = 45,
                Width = 35,
                CornerRadius = new CornerRadius(5),
                BorderBrush = AppColors.Gold4,
                BorderThickness = new Thickness(2),
                Background = AppColors.White,
                Children =
                {
                    SetText("1", 26, Color.FromArgb(255, 200, 155, 60), stretch: Stretch.None),
                    new Rectangle
                    {
                        Width = 25, Height = 2, Fill = new SolidColorBrush(Color.FromArgb(255, 200, 155, 60)),
                        Margin = new Thickness(5, 0, 5, 0)
                    }
                }
            }
        };

        Grid.SetRow(rankingViewbox, 0);
        firstGrid.Children.Add(rankingViewbox);


        var firstProfileIconImage = GetProfileIcon(firstPlayer.SummonerId, 10, 50);
        firstProfileIconImage.BorderBrush = AppColors.Gold4;
        firstProfileIconImage.BorderThickness = new Thickness(2);
        var summonerNameTextBlock = SetText(GetSummonerName(firstPlayer.SummonerId), 14, Colors.White);
        summonerNameTextBlock.FontWeight = new FontWeight(700);

        var firstInfosViewbox = new Viewbox
        {
            Child = new StackPanel
            {
                Spacing = 10,
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
                    GetImage(
                        @"C:\Users\alcam\OneDrive\\Bureau\nexus-client\NexusClient\NexusClient\Assets\emblems\Rank=Challenger.png",
                        0, 70),
                    SetText($"{firstPlayer.LeaguePoints} LP", 14, Colors.White)
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
                Fill = AppColors.Gold4,
                Height = 7
            },
            CornerRadius = new CornerRadius(3)
        });


        var winRateTextBlock = SetText(
            $"{Math.Round(firstPlayer.Wins / (float)(firstPlayer.Wins + firstPlayer.Losses) * 100)} %",
            14, Colors.White);
        winRateTextBlock.Margin = new Thickness(10);

        var gamesPlayedTextBlock = SetText($"{firstPlayer.Wins + firstPlayer.Losses} games",
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
            Padding = new Thickness(5),
            Margin = new Thickness(10, 10, 10, 5),
            CornerRadius = new CornerRadius(12),
            Background = AppColors.GoldGradient
        };

        secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });
        secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        secondGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

        var secondRankingViewbox = new Viewbox
        {
            Child = new StackPanel
            {
                Margin = new Thickness(10),
                Height = 40,
                Width = 35,
                CornerRadius = new CornerRadius(5),
                BorderBrush = AppColors.GreyCool,
                BorderThickness = new Thickness(2),
                Background = AppColors.White,
                Children =
                {
                    SetText("2", 22, Color.FromArgb(255, 200, 155, 60), stretch: Stretch.None),
                    new Rectangle
                    {
                        Width = 25, Height = 2, Fill = new SolidColorBrush(Color.FromArgb(255, 200, 155, 60)),
                        Margin = new Thickness(5, 0, 5, 0)
                    }
                }
            }
        };

        Grid.SetRow(secondRankingViewbox, 0);
        secondGrid.Children.Add(secondRankingViewbox);

        var secondProfileIconImage = GetProfileIcon(secondPlayer.SummonerId, 10, 40);
        var secondSummonerNameTextBlock = SetText(GetSummonerName(secondPlayer.SummonerId),
            14, Colors.White);
        secondSummonerNameTextBlock.FontWeight = new FontWeight(700);

        var secondInfosViewbox = new Viewbox
        {
            Child = new StackPanel
            {
                Spacing = 8,
                Orientation = Orientation.Vertical,
                Children = { secondProfileIconImage, secondSummonerNameTextBlock }
            },
            Margin = new Thickness(7)
        };

        Grid.SetRow(secondInfosViewbox, 0);
        Grid.SetColumn(secondInfosViewbox, 1);
        secondGrid.Children.Add(secondInfosViewbox);


        var source =
            @"C:\Users\alcam\OneDrive\Bureau\nexus-client\NexusClient\NexusClient\Assets\emblems\Rank=Challenger.png";

        var secondPlayerLps = SetText($"{secondPlayer.LeaguePoints} LP",
            14, Colors.White);
        secondPlayerLps.Padding = new Thickness(20, 0, 20, 0);

        var secondEmblemViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Vertical,
                Children =
                {
                    GetImage(source, 0, 40), secondPlayerLps
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

        var secondWinRateTextBlock = SetText(
            $"{Math.Round(secondPlayer.Wins / (float)(secondPlayer.Wins + secondPlayer.Losses) * 100)} %",
            14, Colors.White);
        secondWinRateTextBlock.Margin = new Thickness(10);

        var secondGamesPlayedTextBlock = SetText($"{firstPlayer.Wins + firstPlayer.Losses} parties",
            14, Colors.White);
        secondGamesPlayedTextBlock.Margin = new Thickness(10);

        var secondWinRateViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
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
            Padding = new Thickness(5),
            Margin = new Thickness(10, 5, 10, 10),
            CornerRadius = new CornerRadius(12),
            Background = AppColors.GoldGradient
        };
        thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });
        thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        thirdGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

        var thirdRankingViewbox = new Viewbox
        {
            Child = new StackPanel
            {
                Margin = new Thickness(10),
                Height = 40,
                Width = 35,
                CornerRadius = new CornerRadius(5),
                BorderBrush = AppColors.GreyCool,
                BorderThickness = new Thickness(2),
                Background = AppColors.White,
                Children =
                {
                    SetText("3", 22, Color.FromArgb(255, 200, 155, 60), stretch: Stretch.None),
                    new Rectangle
                    {
                        Width = 25, Height = 2, Fill = new SolidColorBrush(Color.FromArgb(255, 200, 155, 60)),
                        Margin = new Thickness(5, 0, 5, 0)
                    }
                }
            }
        };


        Grid.SetRow(thirdRankingViewbox, 0);
        thirdGrid.Children.Add(thirdRankingViewbox);


        var thirdProfileIconImage = GetProfileIcon(firstPlayer.SummonerId, 10, 40);

        var thirdSummonerNameTextBlock = SetText(GetSummonerName(thirdPlayer.SummonerId),
            14, Colors.White);
        thirdSummonerNameTextBlock.FontWeight = new FontWeight(700);

        var thirdLeaderBoardViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                Spacing = 8,
                Margin = new Thickness(7),
                Children = { thirdProfileIconImage, thirdSummonerNameTextBlock }
            }
        };

        Grid.SetRow(thirdLeaderBoardViewBox, 0);
        Grid.SetColumn(thirdLeaderBoardViewBox, 1);
        thirdGrid.Children.Add(thirdLeaderBoardViewBox);

        source =
            @"C:\Users\alcam\OneDrive\Bureau\nexus-client\NexusClient\NexusClient\Assets\emblems\Rank=Challenger.png";

        var thirdPlayerLps = SetText($"{thirdPlayer.LeaguePoints} LP",
            14, Colors.White);
        thirdPlayerLps.Padding = new Thickness(20, 0, 20, 0);

        var thirdEmblemViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Vertical,
                Children =
                {
                    GetImage(source, 0, 40), thirdPlayerLps
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

        var thirdWinRateTextBlock = SetText($"{thirdPlayer.LeaguePoints} LP",
            14, Colors.White);
        thirdWinRateTextBlock.Margin = new Thickness(10);

        var thirdGamesPlayedTextBlock = SetText($"{firstPlayer.Wins + firstPlayer.Losses} parties",
            14, Colors.White);
        thirdGamesPlayedTextBlock.Margin = new Thickness(10);

        var thirdWinRateViewBox = new Viewbox
        {
            Child = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
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

        var matches = GetLastMatches(LolSummoner.Puuid, 3);
        var i = 0;
        foreach (var match in matches)
        {
            var matchGrid = new Grid
            {
                Background = AppColors.GreyCool,
                Padding = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                CornerRadius = new CornerRadius(10)
            };

            matchGrid.PointerPressed += Match_OnPointerPressed;
            matchGrid.PointerEntered += MatchGridOnPointerEntered;
            matchGrid.PointerExited += MatchGridOnPointerExited;

            matchGrid.Tag = match;

            for (var j = 0; j < 6; j++)
                matchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            matchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5, GridUnitType.Star) });
            matchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5, GridUnitType.Star) });
            matchGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(7, GridUnitType.Star) });


            foreach (var participant in match.Info.Participants)
                if (participant.SummonerId == LolSummoner.Id)
                {
                    // if (participant.Win)
                    //     matchGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185));
                    // else
                    //     matchGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6));

                    var championIcon = GetChampionImage(participant.ChampionName, 10, 50);
                    championIcon.Margin = new Thickness(10, 0, 0, 0);

                    var champIconViewBox = new Viewbox
                    {
                        Child = championIcon
                    };

                    Grid.SetColumn(champIconViewBox, 0);
                    Grid.SetRow(champIconViewBox, 0);
                    Grid.SetColumnSpan(champIconViewBox, 2);
                    matchGrid.Children.Add(champIconViewBox);

                    var titleChampionTextBlock = SetText(participant.ChampionName, 30, participant.Win ? Colors.Blue : Colors.Red);

                    var gameTimeStampDuration = DateTime.Now -
                                                DateTimeOffset.FromUnixTimeMilliseconds(
                                                    // ReSharper disable once PossibleInvalidOperationException
                                                    (long)match.Info.GameEndTimestamp);


                    var matchWasChampionString = "Il y a \n";
                    if (gameTimeStampDuration.Days != 0)
                        matchWasChampionString += $"{gameTimeStampDuration.Days} jours ";
                    if (gameTimeStampDuration.Hours != 0)
                        matchWasChampionString += $"{gameTimeStampDuration.Hours} heures";


                    var matchWasTextBlock = SetText(matchWasChampionString,
                        14, Colors.White, stretch: Stretch.Uniform);


                    var titleChampionViewBox = new Viewbox
                    {
                        Child = new StackPanel
                        {
                            Padding = new Thickness(8),
                            Orientation = Orientation.Vertical,
                            Children = { titleChampionTextBlock, matchWasTextBlock }
                        },
                        Stretch = Stretch.Uniform
                    };

                    Grid.SetColumn(titleChampionViewBox, 2);
                    Grid.SetRow(titleChampionViewBox, 0);
                    Grid.SetColumnSpan(titleChampionViewBox, 2);

                    matchGrid.Children.Add(titleChampionViewBox);

                    var source = $"ms-appx:///Assets/media/roles-icons/{participant.TeamPosition}.png";
                    Debug.WriteLine(source);
                    var roleLogo = GetImage(source, 0, 40);

                    Grid.SetRow(roleLogo, 0);
                    Grid.SetColumn(roleLogo, 4);
                    Grid.SetColumnSpan(roleLogo, 2);
                    matchGrid.Children.Add(roleLogo);

                    var kdaChampionTextBlock = SetText(
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

                    var kpChampionTextBlock = SetText(
                        $"{Math.Round((float)(participant.Kills + participant.Assists) / teamKills * 100)} % kp",
                        14, Colors.White, stretch: Stretch.Uniform);
                    kpChampionTextBlock.MaxHeight = 30;
                    kpChampionTextBlock.MaxWidth = 60;

                    Grid.SetColumn(kpChampionTextBlock, 2);
                    Grid.SetRow(kpChampionTextBlock, 1);
                    Grid.SetColumnSpan(kpChampionTextBlock, 2);
                    matchGrid.Children.Add(kpChampionTextBlock);

                    var csChampionTextBlock = SetText(
                        $"{participant.TotalMinionsKilled + participant.TotalAllyJungleMinionsKilled + participant.TotalEnemyJungleMinionsKilled} cs",
                        14, Colors.White, stretch: Stretch.Uniform);
                    csChampionTextBlock.MaxHeight = 30;
                    csChampionTextBlock.MaxWidth = 60;

                    Grid.SetColumn(csChampionTextBlock, 4);
                    Grid.SetRow(csChampionTextBlock, 1);
                    Grid.SetColumnSpan(csChampionTextBlock, 2);
                    matchGrid.Children.Add(csChampionTextBlock);

                    var visionChampionTextBlock = SetText($"{participant.VisionScore} vision",
                        14, Colors.White);

                    var summonerChampionGrid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    summonerChampionGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(30, GridUnitType.Pixel) });
                    summonerChampionGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(30, GridUnitType.Pixel) });

                    summonerChampionGrid.RowDefinitions.Add(new RowDefinition
                        { Height = new GridLength(30, GridUnitType.Pixel) });
                    summonerChampionGrid.RowDefinitions.Add(new RowDefinition
                        { Height = new GridLength(30, GridUnitType.Pixel) });

                    var firstSummonerSpellImage = GetSummonerSpellImage(participant.Summoner1Id);
                    firstSummonerSpellImage.CornerRadius = new CornerRadius(7, 7, 0, 0);


                    Grid.SetColumn(firstSummonerSpellImage, 0);
                    Grid.SetRow(firstSummonerSpellImage, 0);
                    summonerChampionGrid.Children.Add(firstSummonerSpellImage);

                    var secondSummonerSpellImage = GetSummonerSpellImage(participant.Summoner2Id);
                    secondSummonerSpellImage.CornerRadius = new CornerRadius(0, 0, 7, 7);


                    Grid.SetColumn(secondSummonerSpellImage, 0);
                    Grid.SetRow(secondSummonerSpellImage, 1);
                    summonerChampionGrid.Children.Add(secondSummonerSpellImage);


                    var perksImages = GetPerks(participant.Perks.Styles[0].Style,
                        participant.Perks.Styles[0].Selections[0].Perk, participant.Perks.Styles[1].Style);


                    Grid.SetColumn(perksImages[0], 1);
                    Grid.SetRow(perksImages[0], 0);
                    summonerChampionGrid.Children.Add(perksImages[0]);


                    Grid.SetColumn(perksImages[1], 1);
                    Grid.SetRow(perksImages[1], 1);
                    summonerChampionGrid.Children.Add(perksImages[1]);

                    var summonersViewbox = new Viewbox
                    {
                        Child = new StackPanel
                        {
                            Margin = new Thickness(10),
                            Children = { visionChampionTextBlock, summonerChampionGrid }
                        },
                        Stretch = Stretch.Uniform
                    };

                    Grid.SetColumn(summonersViewbox, 0);
                    Grid.SetRow(summonersViewbox, 2);
                    Grid.SetColumnSpan(summonersViewbox, 3);
                    matchGrid.Children.Add(summonersViewbox);


                    var gameDuration = DateTimeOffset.FromUnixTimeMilliseconds((long)match.Info.GameEndTimestamp) -
                                       DateTimeOffset.FromUnixTimeMilliseconds(match.Info.GameStartTimestamp);


                    var matchDurationTextBlock = SetText(
                        $"{gameDuration.Minutes} minutes\n{gameDuration.Seconds} secondes",
                        14, Colors.White);
                    matchDurationTextBlock.Margin = new Thickness(8);

                    var itemsChampionGrid = new Grid
                    {
                        CornerRadius = new CornerRadius(10),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    for (var j = 0; j < 4; j++)
                        itemsChampionGrid.ColumnDefinitions.Add(new ColumnDefinition
                            { Width = new GridLength(20, GridUnitType.Pixel) });

                    itemsChampionGrid.RowDefinitions.Add(new RowDefinition
                        { Height = new GridLength(20, GridUnitType.Pixel) });
                    itemsChampionGrid.RowDefinitions.Add(new RowDefinition
                        { Height = new GridLength(20, GridUnitType.Pixel) });

                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item0}.png";
                    var itemImage0 = GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item1}.png";
                    var itemImage1 = GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item2}.png";
                    var itemImage2 = GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item3}.png";
                    var itemImage3 = GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item4}.png";
                    var itemImage4 = GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item5}.png";
                    var itemImage5 = GetImage(source, 3);
                    source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item6}.png";
                    var itemImage6 = GetImage(source, 3);

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

                    var itemsViewbox = new Viewbox
                    {
                        Child = new StackPanel
                        {
                            Children = { matchDurationTextBlock, itemsChampionGrid }
                        },
                        Stretch = Stretch.Uniform
                    };

                    Grid.SetColumn(itemsViewbox, 3);
                    Grid.SetRow(itemsViewbox, 2);
                    Grid.SetColumnSpan(itemsViewbox, 3);

                    matchGrid.Children.Add(itemsViewbox);
                }

            var matchName = SetText(match.Info.GameMode.ToString().ToLower(), 20,
                Color.FromArgb(255, 52, 73, 94));
            var matchStackPanelViewbox = new Viewbox
            {
                Child = new StackPanel
                {
                    Margin = new Thickness(10),
                    Children = { matchName, matchGrid }
                }
            };

            Grid.SetColumn(matchStackPanelViewbox, i);
            MatchListGrid.Children.Add(matchStackPanelViewbox);

            i++;
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

            Frame.Navigate(typeof(MatchDetail), matchGrid.Tag, new DrillInNavigationTransitionInfo());
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
}