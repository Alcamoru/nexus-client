using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
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
public sealed partial class SummonerInfoPage : Page
{
    private Summoner Summoner { get; set; }
    private RiotGamesApi Api { get; set; }

    public SummonerInfoPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not List<object> parameters) return;
        Api = (RiotGamesApi)parameters.ElementAt(0);
        var summonerName = parameters.ElementAt(1).ToString();
        Summoner = Api.SummonerV4().GetBySummonerName(PlatformRoute.EUW1, summonerName!);
        SetLastMatches();
    }

    private List<Match> GetLastMatches()
    {
        List<Match> matches = new List<Match>();
        string[] matchListIds = Api.MatchV5().GetMatchIdsByPUUID(RegionalRoute.EUROPE, Summoner.Puuid, count:3);
        foreach (string matchListId in matchListIds)
        {
            matches.Add(Api.MatchV5().GetMatch(RegionalRoute.EUROPE, matchListId));
        }

        return matches;
    }

    private void SetLastMatches()
    {
        List<Match> matches = GetLastMatches();
        foreach (Match match in matches)
        {
            
            Grid matchGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                CornerRadius = new CornerRadius(10)
            };

            ColumnDefinition col1 = new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)};
            ColumnDefinition col2 = new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)};
            ColumnDefinition col3 = new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)};
            ColumnDefinition col4 = new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)};
            ColumnDefinition col5 = new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)};
            ColumnDefinition col6 = new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)};
            
            matchGrid.ColumnDefinitions.Add(col1);
            matchGrid.ColumnDefinitions.Add(col2);
            matchGrid.ColumnDefinitions.Add(col3);
            matchGrid.ColumnDefinitions.Add(col4);
            matchGrid.ColumnDefinitions.Add(col5);
            matchGrid.ColumnDefinitions.Add(col6);

            RowDefinition row1 = new RowDefinition() { Height = new GridLength(5, GridUnitType.Pixel) };
            RowDefinition row2 = new RowDefinition() { Height = new GridLength(5, GridUnitType.Pixel) };
            RowDefinition row3 = new RowDefinition() { Height = new GridLength(7, GridUnitType.Pixel) };
            
            matchGrid.RowDefinitions.Add(row1);
            matchGrid.RowDefinitions.Add(row2);
            matchGrid.RowDefinitions.Add(row3);
            
            foreach (var participant in match.Info.Participants)
            {
                if (participant.SummonerName == Summoner.Name)
                {
                    if (participant.Win)
                    {
                        matchGrid.Background = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185));
                    }
                    else
                    {
                        matchGrid.Background = new SolidColorBrush(Color.FromArgb(255, 235,47,6));
                    }

                    Image championIcon = new Image()
                    {
                        Source = new BitmapImage(new Uri($"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/champion/{participant.ChampionName}.png")),
                        Width = 40
                    };

                    Border summonerIconBorder = new Border()
                    {
                        CornerRadius = new CornerRadius(10),

                    };
                    summonerIconBorder.Child = championIcon;
                    Grid.SetColumn(summonerIconBorder, 0);
                    Grid.SetRow(summonerIconBorder, 0);
                    Grid.SetColumnSpan(summonerIconBorder, 2);

                    TextBlock titleChampionTextBlock = new TextBlock()
                    {
                        Text = participant.ChampionName,
                        FontSize = 20,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };


                    var gameTimeStampDuration = DateTime.Now -
                                                DateTimeOffset.FromUnixTimeMilliseconds(
                                                    // ReSharper disable once PossibleInvalidOperationException
                                                    (long)match.Info.GameEndTimestamp);
                        Debug.WriteLine(gameTimeStampDuration);

                        string matchWasChampionString = "Il y a";
                        if (gameTimeStampDuration.Days != 0)
                        {
                            matchWasChampionString += $"{gameTimeStampDuration.Days} jours";
                        }
                        if (gameTimeStampDuration.Hours != 0)
                        {
                            matchWasChampionString += $"{gameTimeStampDuration.Days} heures";
                        }
                        if (gameTimeStampDuration.Minutes != 0)
                        {
                            matchWasChampionString += $"{gameTimeStampDuration.Days} minutes";
                        }
                    TextBlock matchWasChampionTextBlock = new TextBlock()
                    {
                        Text = matchWasChampionString,
                        FontSize = 20,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };
                    
                    StackPanel titlesChampionStackPanel = new StackPanel();
                    titlesChampionStackPanel.Children.Add(titleChampionTextBlock);
                    titlesChampionStackPanel.Children.Add(matchWasChampionTextBlock);

                    Grid.SetColumn(titlesChampionStackPanel, 2);
                    Grid.SetRow(titlesChampionStackPanel, 0);
                    Grid.SetColumnSpan(titlesChampionStackPanel, 2);

                    Image roleLogo = new Image()
                    {
                        Width = 40,
                        Source = new BitmapImage(new Uri($"ms-appx:///Assets/media/roles-icons/{participant.Role}"))
                    };
                    Grid.SetRow(roleLogo, 0);
                    Grid.SetColumn(roleLogo, 4);
                    Grid.SetColumnSpan(roleLogo, 2);

                    TextBlock kdaChampionTextBlock = new TextBlock()
                    {
                        TextAlignment = TextAlignment.Center,
                        Text = $"{participant.Kills} | {participant.Deaths} | {participant.Assists}",
                        FontSize = 20,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    Grid.SetColumn(kdaChampionTextBlock, 0);
                    Grid.SetRow(kdaChampionTextBlock, 1);
                    Grid.SetColumnSpan(kdaChampionTextBlock, 2);
                    int teamKills = 1;
                    foreach (Team team in match.Info.Teams)
                    {
                        if (team.TeamId == participant.TeamId)
                        {
                            teamKills = team.Objectives.Champion.Kills;
                        }
                    }

                    TextBlock kpChampionTextBlock = new TextBlock()
                    {
                        TextAlignment = TextAlignment.Center,
                        Text = $"{(participant.Kills + participant.Assists) / teamKills}",
                        FontSize = 20,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    Grid.SetColumn(kdaChampionTextBlock, 2);
                    Grid.SetRow(kdaChampionTextBlock, 1);
                    Grid.SetColumnSpan(kdaChampionTextBlock, 2);

                    TextBlock csChampionTextBlock = new TextBlock()
                    {
                        TextAlignment = TextAlignment.Center,
                        Text = $"{participant.TotalMinionsKilled}",
                        FontSize = 20,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    Grid.SetColumn(kdaChampionTextBlock, 4);
                    Grid.SetRow(kdaChampionTextBlock, 1);
                    Grid.SetColumnSpan(kdaChampionTextBlock, 2);


                    StackPanel summonersStackPanel = new StackPanel();
                    TextBlock visionChampionTextBlock = new TextBlock()
                    {
                        TextAlignment = TextAlignment.Center,
                        Text = $"{participant.VisionScore}",
                        FontSize = 20,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    Grid summonerChampionGrid = new Grid();

                    ColumnDefinition summonerChampionGridColumn1 = new ColumnDefinition()
                        { Width = new GridLength(30, GridUnitType.Pixel) };
                    ColumnDefinition summonerChampionGridColumn2 = new ColumnDefinition()
                        { Width = new GridLength(30, GridUnitType.Pixel) };

                    summonerChampionGrid.ColumnDefinitions.Add(summonerChampionGridColumn1);
                    summonerChampionGrid.ColumnDefinitions.Add(summonerChampionGridColumn2);

                    RowDefinition summonerChampionGridRow1 = new RowDefinition()
                        { Height = new GridLength(30, GridUnitType.Pixel) };
                    RowDefinition summonerChampionGridRow2 = new RowDefinition()
                        { Height = new GridLength(30, GridUnitType.Pixel) };

                    summonerChampionGrid.RowDefinitions.Add(summonerChampionGridRow1);
                    summonerChampionGrid.RowDefinitions.Add(summonerChampionGridRow2);

                    Image firstSummonerSpellImage = new Image()
                    {
                        Source = new BitmapImage(new Uri(""))
                    };

                    Grid.SetColumn(firstSummonerSpellImage, 0);
                    Grid.SetRow(firstSummonerSpellImage, 0);

                    Image secondSummonerSpellImage = new Image()
                    {
                        Source = new BitmapImage(new Uri(""))
                    };

                    Grid.SetColumn(secondSummonerSpellImage, 0);
                    Grid.SetRow(secondSummonerSpellImage, 1);

                    Image mainRune = new Image()
                    {
                        Source = new BitmapImage(new Uri(""))
                    };

                    Grid.SetColumn(mainRune, 1);
                    Grid.SetRow(mainRune, 0);

                    Image secondaryRune = new Image()
                    {
                        Source = new BitmapImage(new Uri(""))
                    };

                    Grid.SetColumn(secondaryRune, 1);
                    Grid.SetRow(secondaryRune, 1);


                    summonersStackPanel.Children.Add(visionChampionTextBlock);
                    summonersStackPanel.Children.Add(summonerChampionGrid);

                    Grid.SetColumn(summonersStackPanel, 0);
                    Grid.SetRow(summonersStackPanel, 2);
                    Grid.SetColumnSpan(summonersStackPanel, 3);


                    StackPanel itemsChampionStackPanel = new StackPanel();
                    TextBlock matchDurationChampionTextBlock = new TextBlock()
                    {
                        TextAlignment = TextAlignment.Center,
                        Text = $"{match.Info.GameDuration}",
                        FontSize = 20,
                        FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                    };

                    Grid itemsChampionGrid = new Grid()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    ColumnDefinition itemColumn1 = new ColumnDefinition() {Width = new GridLength(20, GridUnitType.Pixel)};
                    ColumnDefinition itemColumn2 = new ColumnDefinition() {Width = new GridLength(20, GridUnitType.Pixel)};
                    ColumnDefinition itemColumn3 = new ColumnDefinition() {Width = new GridLength(20, GridUnitType.Pixel)};
                    ColumnDefinition itemColumn4 = new ColumnDefinition() {Width = new GridLength(20, GridUnitType.Pixel)};

                    itemsChampionGrid.ColumnDefinitions.Add(itemColumn1);
                    itemsChampionGrid.ColumnDefinitions.Add(itemColumn2);
                    itemsChampionGrid.ColumnDefinitions.Add(itemColumn3);
                    itemsChampionGrid.ColumnDefinitions.Add(itemColumn4);

                    RowDefinition itemRow1 = new RowDefinition() { Height = new GridLength(20, GridUnitType.Pixel) };
                    RowDefinition itemRow2 = new RowDefinition() { Height = new GridLength(20, GridUnitType.Pixel) };

                    Image itemImage0 = new Image()
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/item/{participant.Item0}.png"))
                    };

                    Grid.SetColumn(itemImage0, 0);
                    Grid.SetRow(itemImage0, 0);

                    Image itemImage1 = new Image()
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/item/{participant.Item1}.png"))
                    };

                    Grid.SetColumn(itemImage1, 1);
                    Grid.SetRow(itemImage0, 0);

                    Image itemImage2 = new Image()
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/item/{participant.Item2}.png"))
                    };

                    Grid.SetColumn(itemImage2, 2);
                    Grid.SetRow(itemImage2, 0);

                    Image itemImage3 = new Image()
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/item/{participant.Item3}.png"))
                    };

                    Grid.SetColumn(itemImage3, 3);
                    Grid.SetRow(itemImage3, 0);

                    Image itemImage4 = new Image()
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/item/{participant.Item4}.png"))
                    };

                    Grid.SetColumn(itemImage4, 0);
                    Grid.SetRow(itemImage4, 1);

                    Image itemImage5 = new Image()
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/item/{participant.Item5}.png"))
                    };

                    Grid.SetColumn(itemImage5, 1);
                    Grid.SetRow(itemImage5, 1);

                    Image itemImage6 = new Image()
                    {
                        Source = new BitmapImage(new Uri(
                            $"http://ddragon.leagueoflegends.com/cdn/13.21.1/img/item/{participant.Item6}.png"))
                    };

                    Grid.SetColumn(itemImage6, 2);
                    Grid.SetRow(itemImage6, 1);

                    itemsChampionStackPanel.Children.Add(matchDurationChampionTextBlock);
                    itemsChampionStackPanel.Children.Add(itemsChampionGrid);

                    Grid.SetColumn(itemsChampionStackPanel, 3);
                    Grid.SetRow(itemsChampionStackPanel, 2);
                    Grid.SetColumnSpan(itemsChampionStackPanel, 3);
                }
            }
        }
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(WelcomePage));
    }
}