using System;
using System.Collections.Generic;
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
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PlayerInfo : Page
{
    public PlayerInfo()
    {
        InitializeComponent();
    }

    private RiotGamesApi Api { get; set; }

    private Summoner LolSummoner { get; set; }

    private Summoner SummonerInfo { get; set; }

    private RegionalRoute SummonerRegionalRoute { get; set; }

    private PlatformRoute SummonerPlatformRoute { get; set; }

    private UtilisMethods Methods { get; set; }

    private List<Match> Matches { get; set; }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not List<object> parameters) return;
        Api = (RiotGamesApi)parameters.ElementAt(0);
        SummonerInfo = (Summoner)parameters.ElementAt(1);
        LolSummoner = (Summoner)parameters.ElementAt(2);
        SummonerRegionalRoute = (RegionalRoute)parameters.ElementAt(3);
        SummonerPlatformRoute = (PlatformRoute)parameters.ElementAt(4);
        Methods = new UtilisMethods(Api, LolSummoner, SummonerRegionalRoute, SummonerPlatformRoute);
        base.OnNavigatedTo(e);
        Matches = Methods.GetLastMatches(SummonerInfo.Puuid, 20);
        SetBestChampions();
        SetLastMatches();
    }

    private void SetBestChampions()
    {
        Dictionary<string, List<Participant>> championsPlayed = new Dictionary<string, List<Participant>>();
        foreach (var match in Matches)
        {
            foreach (Participant participant in match.Info.Participants)
            {
                if (participant.SummonerId == SummonerInfo.Id)
                {
                    if (championsPlayed.Keys.Contains(participant.ChampionName))
                    {
                        championsPlayed[participant.ChampionName].Add(participant);
                    }
                    else
                    {
                        championsPlayed[participant.ChampionName] = new List<Participant>() { participant };
                    }
                }
            }
        }

        var sortedChampionsPlayed = from entry in championsPlayed orderby entry.Value.Count descending select entry;
        int i = 0;
        foreach (KeyValuePair<string,List<Participant>> keyValuePair in sortedChampionsPlayed)
        {
            Grid championGrid = new Grid()
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition() {Width = new GridLength(2, GridUnitType.Star)},
                    new ColumnDefinition() {Width = new GridLength(2, GridUnitType.Star)},
                    new ColumnDefinition() {Width = new GridLength(2, GridUnitType.Star)}
                },
                RowDefinitions =
                {
                    new RowDefinition(){Height = new GridLength(1, GridUnitType.Star)},
                    new RowDefinition(){Height = new GridLength(1, GridUnitType.Star)}
                },
                Padding = new Thickness(8),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1)
            };

            var championImage = Methods.GetChampionImage(keyValuePair.Key);
            Grid.SetColumn(championImage, 0);
            Grid.SetRowSpan(championImage, 2);
            championGrid.Children.Add(championImage);

            var championName = Methods.SetText(keyValuePair.Key, 14, Colors.Black);
            Grid.SetColumn(championName, 1);
            championGrid.Children.Add(championName);

            var averageCs = 0;
            var averageKda = 0;
            var averageKills = 0;
            var averageDeaths = 0;
            var averageAssists= 0;
            int matchNumber = 0;
            int wins = 0;
            int losses = 0;
            foreach (Participant participant in keyValuePair.Value)
            {
                averageCs += participant.TotalMinionsKilled + (int)participant.TotalAllyJungleMinionsKilled! +
                             (int)participant.TotalEnemyJungleMinionsKilled!;
                if (participant.Deaths != 0)
                {
                    averageKda += (participant.Kills + participant.Assists) / participant.Deaths;
                }
                else
                {
                    averageKda += (participant.Kills + participant.Assists);
                }
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

            var averageCsTextBlock = Methods.SetText($"CS {averageCs}", 14, Colors.Black);
            Grid.SetColumn(averageCsTextBlock, 1);
            Grid.SetRow(averageCsTextBlock, 1);
            championGrid.Children.Add(averageCsTextBlock);

            var averageKdaTextBlock = Methods.SetText($"{averageKda} KDA", 14, Colors.Black);
            Grid.SetColumn(averageKdaTextBlock, 2);
            Grid.SetRow(averageKdaTextBlock, 0);
            championGrid.Children.Add(averageKdaTextBlock);
            var averageKillsDeathsAssistsTextBlock = Methods.SetText($"{averageKills}/{averageDeaths}/{averageAssists}", 14, Colors.Black);
            Grid.SetColumn(averageKillsDeathsAssistsTextBlock, 2);
            Grid.SetRow(averageKillsDeathsAssistsTextBlock, 1);
            championGrid.Children.Add(averageKillsDeathsAssistsTextBlock);
            var winrateTextblock = Methods.SetText($"{(wins / (wins + losses)) * 100} %", 14, Colors.Black);
            Grid.SetColumn(winrateTextblock, 3);
            Grid.SetRow(winrateTextblock, 0);
            championGrid.Children.Add(winrateTextblock);

            var gamesPlayedTextBlock = Methods.SetText($"{wins + losses} jouées", 14, Colors.Black);
            Grid.SetColumn(gamesPlayedTextBlock, 3);
            Grid.SetRow(gamesPlayedTextBlock, 1);
            championGrid.Children.Add(gamesPlayedTextBlock);

            BestChampionsGrid.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(75, GridUnitType.Pixel)});
            Grid.SetRow(championGrid, i);
            BestChampionsGrid.Children.Add(championGrid);
            i++;
        }

    }

    private void SetLastMatches()
    {
        int i = 0;
        foreach (Match match in Matches)
        {
            Grid matchGrid = new Grid()
            {
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(8),
                ColumnDefinitions =
                {
                    new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition() {Width = new GridLength(2, GridUnitType.Star)},
                    new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition() {Width = new GridLength(1, GridUnitType.Star)},
                    },
                RowDefinitions = { new RowDefinition(),new RowDefinition(),new RowDefinition(),new RowDefinition(), },
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1)
            };

            var gameName = Methods.SetText(match.Info.GameMode.ToString().ToLower(), 11, Colors.White);

            var matchWasTimeStamp = DateTime.Now -
                                        DateTimeOffset.FromUnixTimeMilliseconds(
                                            // ReSharper disable once PossibleInvalidOperationException
                                            (long)match.Info.GameEndTimestamp);
            string matchWas;

            if (matchWasTimeStamp.Days > 30)
            {
                matchWas = $"Il y a {Math.Round((decimal)matchWasTimeStamp.Days / 30)} mois";
            }
            else if (matchWasTimeStamp.Days != 0)
            {
                matchWas = $"Il y a {matchWasTimeStamp.Days} jours";
            }
            else
            {
                matchWas = $"Il y a {matchWasTimeStamp.Hours} heures";
            }

            var matchWasTextBlock = Methods.SetText(matchWas, 11, Colors.White);

            var gameTimeStampDuration = DateTimeOffset.FromUnixTimeSeconds(match.Info.GameDuration);
            var gameDuration = Methods.SetText($"{gameTimeStampDuration.Minute}:{gameTimeStampDuration.Second}", 11, Colors.White);


            foreach (Participant participant in match.Info.Participants)
            {
                if (participant.SummonerId == SummonerInfo.Id)
                {
                    Viewbox winTextBlock;

                    if (participant.Win)
                    {
                        matchGrid.Background = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185));
                        winTextBlock = Methods.SetText("Victoire", 11, Colors.White);
                    }
                    else
                    {
                        matchGrid.Background = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6));
                        winTextBlock = Methods.SetText("Défaite", 11, Colors.White);
                    }

                    Viewbox testViewbox = new Viewbox()
                    {
                        Child = new StackPanel()
                        {
                            Orientation = Orientation.Vertical,
                            Children = { gameName, matchWasTextBlock, gameDuration, winTextBlock }
                        }
                    };

                    Grid.SetRowSpan(testViewbox, 4);
                    matchGrid.Children.Add(testViewbox);

                    var championImage = Methods.GetChampionImage(participant.ChampionName, 5);
                    Grid championGrid = new Grid()
                    {
                        CornerRadius = new CornerRadius(5),
                        RowDefinitions = { new RowDefinition(), new RowDefinition(), },
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(),
                            new ColumnDefinition(),
                        }
                    };

                    Grid.SetRowSpan(championImage, 2);
                    Grid.SetColumnSpan(championImage, 2);
                    championGrid.Children.Add(championImage);

                    var firstSummonerSpellImage = Methods.GetSummonerSpellImage(participant.Summoner1Id);
                    firstSummonerSpellImage.CornerRadius = new CornerRadius(7, 7, 0, 0);


                    Grid.SetColumn(firstSummonerSpellImage, 2);
                    Grid.SetRow(firstSummonerSpellImage, 0);
                    championGrid.Children.Add(firstSummonerSpellImage);

                    var secondSummonerSpellImage = Methods.GetSummonerSpellImage(participant.Summoner2Id);
                    secondSummonerSpellImage.CornerRadius = new CornerRadius(0, 0, 7, 7);


                    Grid.SetColumn(secondSummonerSpellImage, 2);
                    Grid.SetRow(secondSummonerSpellImage, 1);
                    championGrid.Children.Add(secondSummonerSpellImage);

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

                    var mainRune = new Image
                    {
                        Source = new BitmapImage(new Uri(mainRuneUrl))
                    };

                    Grid.SetColumn(mainRune, 3);
                    Grid.SetRow(mainRune, 0);
                    championGrid.Children.Add(mainRune);


                    var secondaryRune = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            $"https://ddragon.leagueoflegends.com/cdn/img/{secondPerkIcon}"))
                    };

                    Grid.SetColumn(secondaryRune, 3);
                    Grid.SetRow(secondaryRune, 1);
                    championGrid.Children.Add(secondaryRune);

                    Grid.SetColumn(championGrid, 1);
                    Grid.SetRowSpan(championGrid, 4);
                    matchGrid.Children.Add(championGrid);

                    var killsDeathsAssistsTextBlock =
                        Methods.SetText($"{participant.Kills}/{participant.Deaths}/{participant.Assists}", 14,
                            Colors.White);
                    Grid.SetColumn(killsDeathsAssistsTextBlock, 2);
                    Grid.SetRow(killsDeathsAssistsTextBlock, 0);
                    matchGrid.Children.Add(killsDeathsAssistsTextBlock);
                    Viewbox kdaTextBlock;
                    if (participant.Deaths != 0)
                    {
                        kdaTextBlock =
                            Methods.SetText($"{(participant.Kills + participant.Assists) / participant.Deaths} KDA", 14,
                                Colors.White);
                    }
                    else
                    {
                        kdaTextBlock =
                            Methods.SetText($"{(participant.Kills + participant.Assists)} KDA", 14,
                                Colors.White);
                    }

                    Grid.SetColumn(kdaTextBlock, 2);
                    Grid.SetRow(kdaTextBlock, 1);
                    matchGrid.Children.Add(kdaTextBlock);

                    var csTextBlock =
                        Methods.SetText($"{participant.TotalMinionsKilled + participant.TotalAllyJungleMinionsKilled + participant.TotalEnemyJungleMinionsKilled} CS", 14,
                            Colors.White);
                    Grid.SetColumn(csTextBlock, 2);
                    Grid.SetRow(csTextBlock, 2);
                    matchGrid.Children.Add(csTextBlock);

                    var visionTextBlock =
                        Methods.SetText($"{participant.VisionScore}", 14,
                            Colors.White);
                    Grid.SetColumn(visionTextBlock, 2);
                    Grid.SetRow(visionTextBlock, 3);
                    matchGrid.Children.Add(visionTextBlock);

                    var itemsGrid = new Grid
                    {
                        CornerRadius = new CornerRadius(10),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    for (int j = 0; j < 4; j++)
                    {
                        itemsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20, GridUnitType.Pixel) });
                    }

                    itemsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20, GridUnitType.Pixel) });
                    itemsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20, GridUnitType.Pixel) });

                    var source = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/item/{participant.Item0}.png";
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
            }


            MatchesGrid.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(75, GridUnitType.Pixel)});
            Grid.SetRow(matchGrid, i);
            MatchesGrid.Children.Add(matchGrid);
            i ++;
        }
    }


    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.GoBack(new DrillInNavigationTransitionInfo());
    }
}