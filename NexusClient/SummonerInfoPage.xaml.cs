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
                    

                }
            }
        }
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(WelcomePage));
    }
}