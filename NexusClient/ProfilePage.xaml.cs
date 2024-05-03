using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using static NexusClient.UtilisMethods;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NexusClient;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ProfilePage : Page
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    private Summoner Player { get; set; }
    private List<Match> Matches { get; set; }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Player = (Summoner)e.Parameter;
        base.OnNavigatedTo(e);
        Matches = GetLastMatches(Player.Puuid, 20);
        SetBestChampions();
        SetLastMatches();
    }

    private void SetBestChampions()
    {
        var championsPlayed = new Dictionary<string, List<Participant>>();
        foreach (var match in Matches)
        foreach (var participant in match.Info.Participants)
            if (participant.SummonerId == Player.Id)
            {
                if (championsPlayed.Keys.Contains(participant.ChampionName))
                    championsPlayed[participant.ChampionName].Add(participant);
                else
                    championsPlayed[participant.ChampionName] = new List<Participant> { participant };
            }

        var sortedChampionsPlayed = from entry in championsPlayed orderby entry.Value.Count descending select entry;
        var i = 0;

        foreach (var keyValuePair in sortedChampionsPlayed)
        {
            var championGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                },
                Padding = new Thickness(8),
                Background = AppColors.Gold4,
                CornerRadius = new CornerRadius(10),
                ColumnSpacing = 7
            };

            Grid championInfosGrid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition() { Height = new GridLength(3, GridUnitType.Star) },
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }
                }
            };

            var championImage = GetChampionImage(keyValuePair.Key, 7, 40);
            Grid.SetRow(championImage, 0);
            championInfosGrid.Children.Add(championImage);

            var championName = new Viewbox()
            {
                Child = SetTextWithViewbox(keyValuePair.Key, 14, Colors.White)
            };
            Grid.SetRow(championName, 1);
            championInfosGrid.Children.Add(championName);

            var championInfos = new Viewbox()
            {
                Child = championInfosGrid
            };

            Grid.SetRowSpan(championInfos, 2);
            championGrid.Children.Add(championInfos);

            var averageCs = 0;
            var averageKda = 0;
            var averageKills = 0;
            var averageDeaths = 0;
            var averageAssists = 0;
            var matchNumber = 0;
            var wins = 0;
            var losses = 0;
            foreach (var participant in keyValuePair.Value)
            {
                averageCs += participant.TotalMinionsKilled + (int)participant.TotalAllyJungleMinionsKilled! +
                             (int)participant.TotalEnemyJungleMinionsKilled!;
                if (participant.Deaths != 0)
                    averageKda += (participant.Kills + participant.Assists) / participant.Deaths;
                else
                    averageKda += participant.Kills + participant.Assists;
                averageKills += participant.Kills;
                averageDeaths += participant.Deaths;
                averageAssists += participant.Assists;
                if (participant.Win) wins += 1;
                else losses += 1;
                matchNumber++;
            }

            averageCs /= matchNumber;
            averageKda /= matchNumber;
            averageKills /= matchNumber;
            averageDeaths /= matchNumber;
            averageAssists /= matchNumber;



            var averageCsTextBlock = SetTextWithViewbox($"{averageCs}", 14, Colors.White);
            averageCsTextBlock.Margin = new Thickness(0, 15, 0, 15);
            Grid.SetColumn(averageCsTextBlock, 1);
            Grid.SetRow(averageCsTextBlock, 0);
            Grid.SetRowSpan(averageCsTextBlock, 2);
            championGrid.Children.Add(averageCsTextBlock);

            var averageKdaTextBlock = SetTextWithViewbox($"{averageKda} KDA", 14, Colors.White);
            averageKdaTextBlock.Margin = new Thickness(0, 15, 0, 15);
            Grid.SetColumn(averageKdaTextBlock, 2);
            Grid.SetRow(averageKdaTextBlock, 0);
            Grid.SetRowSpan(averageKdaTextBlock, 2);
            championGrid.Children.Add(averageKdaTextBlock);
            var averageKillsDeathsAssistsTextBlock =
                SetTextWithViewbox($"{averageKills}/{averageDeaths}/{averageAssists}", 14, Colors.White);
            averageKillsDeathsAssistsTextBlock.Margin = new Thickness(0, 15, 0, 15);
            Grid.SetColumn(averageKillsDeathsAssistsTextBlock, 3);
            Grid.SetRow(averageKillsDeathsAssistsTextBlock, 0);
            Grid.SetRowSpan(averageKillsDeathsAssistsTextBlock, 2);
            championGrid.Children.Add(averageKillsDeathsAssistsTextBlock);
            var winrateTextblock = SetTextWithViewbox($"{Math.Round((float)(wins / (wins + losses) * 100), 2)} %", 14, Colors.White);
            Grid.SetColumn(winrateTextblock, 4);
            Grid.SetRow(winrateTextblock, 0);
            championGrid.Children.Add(winrateTextblock);

            var gamesPlayedTextBlock = SetTextWithViewbox($"{wins + losses} jouées", 14, Colors.White);
            Grid.SetColumn(gamesPlayedTextBlock, 4);
            Grid.SetRow(gamesPlayedTextBlock, 1);
            championGrid.Children.Add(gamesPlayedTextBlock);

            BestChampionsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength( 75, GridUnitType.Pixel) });
            Grid.SetRow(championGrid, i);
            BestChampionsGrid.Children.Add(championGrid);
            i++;
        }
    }

    private void SetLastMatches()
    {
        var i = 0;
        foreach (var match in Matches)
        {
            var matchGrid = new Grid
            {
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(8),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                RowDefinitions = { new RowDefinition(), new RowDefinition(), new RowDefinition(), new RowDefinition() },
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1)
            };

            var gameName = SetTextWithViewbox(match.Info.GameMode.ToString().ToLower(), 11, Colors.White);

            var matchWasTimeStamp = DateTime.Now -
                                    DateTimeOffset.FromUnixTimeMilliseconds(
                                        // ReSharper disable once PossibleInvalidOperationException
                                        (long)match.Info.GameEndTimestamp);
            string matchWas;

            if (matchWasTimeStamp.Days > 30)
                matchWas = $"Il y a {Math.Round((decimal)matchWasTimeStamp.Days / 30)} mois";
            else if (matchWasTimeStamp.Days != 0)
                matchWas = $"Il y a {matchWasTimeStamp.Days} jours";
            else
                matchWas = $"Il y a {matchWasTimeStamp.Hours} heures";

            var matchWasTextBlock = SetTextWithViewbox(matchWas, 11, Colors.White);

            var gameTimeStampDuration = DateTimeOffset.FromUnixTimeSeconds(match.Info.GameDuration);
            var gameDuration = SetTextWithViewbox($"{gameTimeStampDuration.Minute}:{gameTimeStampDuration.Second}", 11,
                Colors.White);


            foreach (var participant in match.Info.Participants)
                if (participant.SummonerId == Player.Id)
                {
                    Viewbox winTextBlock;

                    if (participant.Win)
                    {
                        matchGrid.Background = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185));
                        winTextBlock = SetTextWithViewbox("Victoire", 11, Colors.White);
                    }
                    else
                    {
                        matchGrid.Background = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6));
                        winTextBlock = SetTextWithViewbox("Défaite", 11, Colors.White);
                    }

                    var testViewbox = new Viewbox
                    {
                        Child = new StackPanel
                        {
                            Orientation = Orientation.Vertical,
                            Children = { gameName, matchWasTextBlock, gameDuration, winTextBlock }
                        }
                    };

                    Grid.SetRowSpan(testViewbox, 4);
                    matchGrid.Children.Add(testViewbox);

                    var championImage = GetChampionImage(participant.ChampionName, 5);
                    var championGrid = new Grid
                    {
                        CornerRadius = new CornerRadius(5),
                        RowDefinitions = { new RowDefinition(), new RowDefinition() },
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(),
                            new ColumnDefinition()
                        }
                    };

                    Grid.SetRowSpan(championImage, 2);
                    Grid.SetColumnSpan(championImage, 2);
                    championGrid.Children.Add(championImage);

                    var firstSummonerSpellImage = GetSummonerSpellImage(participant.Summoner1Id);
                    firstSummonerSpellImage.CornerRadius = new CornerRadius(7, 7, 0, 0);


                    Grid.SetColumn(firstSummonerSpellImage, 2);
                    Grid.SetRow(firstSummonerSpellImage, 0);
                    championGrid.Children.Add(firstSummonerSpellImage);

                    var secondSummonerSpellImage = GetSummonerSpellImage(participant.Summoner2Id);
                    secondSummonerSpellImage.CornerRadius = new CornerRadius(0, 0, 7, 7);


                    Grid.SetColumn(secondSummonerSpellImage, 2);
                    Grid.SetRow(secondSummonerSpellImage, 1);
                    championGrid.Children.Add(secondSummonerSpellImage);

                    var perksImages = GetPerks(participant.Perks.Styles[0].Style,
                        participant.Perks.Styles[0].Selections[0].Perk, participant.Perks.Styles[1].Style);
                    Grid.SetColumn(perksImages[0], 3);
                    Grid.SetRow(perksImages[0], 0);
                    championGrid.Children.Add(perksImages[0]);

                    Grid.SetColumn(perksImages[1], 3);
                    Grid.SetRow(perksImages[1], 1);
                    championGrid.Children.Add(perksImages[1]);

                    Grid.SetColumn(championGrid, 1);
                    Grid.SetRowSpan(championGrid, 4);
                    matchGrid.Children.Add(championGrid);

                    var killsDeathsAssistsTextBlock =
                        SetTextWithViewbox($"{participant.Kills}/{participant.Deaths}/{participant.Assists}", 14,
                            Colors.White);
                    Grid.SetColumn(killsDeathsAssistsTextBlock, 2);
                    Grid.SetRow(killsDeathsAssistsTextBlock, 0);
                    matchGrid.Children.Add(killsDeathsAssistsTextBlock);
                    Viewbox kdaTextBlock;
                    if (participant.Deaths != 0)
                        kdaTextBlock =
                            SetTextWithViewbox($"{(participant.Kills + participant.Assists) / participant.Deaths} KDA", 14,
                                Colors.White);
                    else
                        kdaTextBlock =
                            SetTextWithViewbox($"{participant.Kills + participant.Assists} KDA", 14,
                                Colors.White);

                    Grid.SetColumn(kdaTextBlock, 2);
                    Grid.SetRow(kdaTextBlock, 1);
                    matchGrid.Children.Add(kdaTextBlock);

                    var csTextBlock =
                        SetTextWithViewbox(
                            $"{participant.TotalMinionsKilled + participant.TotalAllyJungleMinionsKilled + participant.TotalEnemyJungleMinionsKilled} CS",
                            14,
                            Colors.White);
                    Grid.SetColumn(csTextBlock, 2);
                    Grid.SetRow(csTextBlock, 2);
                    matchGrid.Children.Add(csTextBlock);

                    var visionTextBlock =
                        SetTextWithViewbox($"{participant.VisionScore}", 14,
                            Colors.White);
                    Grid.SetColumn(visionTextBlock, 2);
                    Grid.SetRow(visionTextBlock, 3);
                    matchGrid.Children.Add(visionTextBlock);

                    var itemsGrid = new Grid
                    {
                        CornerRadius = new CornerRadius(10),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    for (var j = 0; j < 4; j++)
                        itemsGrid.ColumnDefinitions.Add(new ColumnDefinition
                            { Width = new GridLength(20, GridUnitType.Pixel) });

                    itemsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20, GridUnitType.Pixel) });
                    itemsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20, GridUnitType.Pixel) });

                    var source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item0}.png";
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
                    itemsGrid.Children.Add(itemImage0);
                    Grid.SetColumn(itemImage1, 1);
                    Grid.SetRow(itemImage0, 0);
                    itemsGrid.Children.Add(itemImage1);
                    Grid.SetColumn(itemImage2, 2);
                    Grid.SetRow(itemImage2, 0);
                    itemsGrid.Children.Add(itemImage2);
                    Grid.SetColumn(itemImage3, 3);
                    Grid.SetRow(itemImage3, 0);
                    itemsGrid.Children.Add(itemImage3);
                    Grid.SetColumn(itemImage4, 0);
                    Grid.SetRow(itemImage4, 1);
                    itemsGrid.Children.Add(itemImage4);
                    Grid.SetColumn(itemImage5, 1);
                    Grid.SetRow(itemImage5, 1);
                    itemsGrid.Children.Add(itemImage5);
                    Grid.SetColumn(itemImage6, 2);
                    Grid.SetRow(itemImage6, 1);
                    itemsGrid.Children.Add(itemImage6);

                    Grid.SetColumn(itemsGrid, 3);
                    Grid.SetRowSpan(itemsGrid, 4);
                    matchGrid.Children.Add(itemsGrid);
                }


            MatchesGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(75, GridUnitType.Pixel) });
            Grid.SetRow(matchGrid, i);
            MatchesGrid.Children.Add(matchGrid);
            i++;
        }
    }
}