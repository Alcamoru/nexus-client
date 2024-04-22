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

    private Match MatchInfo { get; set; }

    private RegionalRoute SummonerRegionalRoute { get; set; }

    private PlatformRoute SummonerPlatformRoute { get; set; }

    private UtilisMethods Methods { get; set; }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not List<object> parameters) return;
        Api = (RiotGamesApi)parameters.ElementAt(0);
        MatchInfo = (Match)parameters.ElementAt(1);
        LolSummoner = (Summoner)parameters.ElementAt(2);
        SummonerRegionalRoute = (RegionalRoute)parameters.ElementAt(3);
        SummonerPlatformRoute = (PlatformRoute)parameters.ElementAt(4);
        Methods = new UtilisMethods(Api, LolSummoner, SummonerRegionalRoute, SummonerPlatformRoute);
        base.OnNavigatedTo(e);
    }

    private List<Match> GetLastMatches()
    {
        var matches = new List<Match>();
        var matchListIds = Api.MatchV5().GetMatchIdsByPUUID(SummonerRegionalRoute, LolSummoner.Puuid, 5);
        foreach (var matchListId in matchListIds)
            matches.Add(Api.MatchV5().GetMatch(SummonerRegionalRoute, matchListId));

        return matches;
    }

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
                BorderThickness = new Thickness(5, 1, 1, 1),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                CornerRadius = new CornerRadius(8)
            };

            matchGrid.PointerPressed += Match_OnPointerPressed;
            matchGrid.PointerEntered += MatchGridOnPointerEntered;
            matchGrid.PointerExited += MatchGridOnPointerExited;

            matchGrid.Tag = match;

            var col1 = new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) };
            var col2 = new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) };
            var col3 = new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) };
            var col4 = new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) };

            matchGrid.ColumnDefinitions.Add(col1);
            matchGrid.ColumnDefinitions.Add(col2);
            matchGrid.ColumnDefinitions.Add(col3);
            matchGrid.ColumnDefinitions.Add(col4);
            Participant summonerPlayer = null;

            foreach (var participant in match.Info.Participants)
                if (participant.Puuid == LolSummoner.Puuid)
                    summonerPlayer = participant;

            if (summonerPlayer!.Win)
            {
                matchGrid.Background = new SolidColorBrush(Color.FromArgb(100, 41, 128, 185));
                var matchWinOrLose = Methods.SetText("Victoire", 18, Colors.Gray);
            }
            else
            {
                matchGrid.Background = new SolidColorBrush(Color.FromArgb(100, 235, 47, 6));
                var matchWinOrLose = Methods.SetText("Défaite", 18, Colors.Gray);
            }

            var matchDuration = TimeSpan.FromMilliseconds(match.Info.GameDuration);


            var gameTimeStampDuration = DateTime.Now -
                                        DateTimeOffset.FromUnixTimeMilliseconds(
                                            // ReSharper disable once PossibleInvalidOperationException
                                            (long)match.Info.GameEndTimestamp);

            var matchWasString = $"Il y a {gameTimeStampDuration.Days} jours";

            var matchType = Methods.SetText("Match classé solo", 18, Color.FromArgb(150, 235, 47, 6));
            var matchWas = Methods.SetText(matchWasString, 18, Colors.Gray);
            var line = new Line { Width = 40 };
            var matchDurationViewbox = Methods.SetText($"{matchDuration.Minutes}:{matchDuration.Seconds}", 16,
                Color.FromArgb(150, 235, 47, 6));
            var matchInfoStackLayout = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children = { matchType, matchWas, line, matchDurationViewbox }
            };

            Grid.SetColumn(matchInfoStackLayout, 0);
            matchGrid.Children.Add(matchInfoStackLayout);

            var gameItemsGrid = new Grid();
            col1 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            col2 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            col3 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            col4 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col5 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col6 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            var col7 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };

            gameItemsGrid.ColumnDefinitions.Add(col1);
            gameItemsGrid.ColumnDefinitions.Add(col2);
            gameItemsGrid.ColumnDefinitions.Add(col3);
            gameItemsGrid.ColumnDefinitions.Add(col4);
            gameItemsGrid.ColumnDefinitions.Add(col5);
            gameItemsGrid.ColumnDefinitions.Add(col6);
            gameItemsGrid.ColumnDefinitions.Add(col7);

            var row1 = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            var row2 = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
            var row3 = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };

            gameItemsGrid.RowDefinitions.Add(row1);
            gameItemsGrid.RowDefinitions.Add(row2);
            gameItemsGrid.RowDefinitions.Add(row3);

            var source = $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/item/{summonerPlayer.Item0}.png";
            var itemImage0 = Methods.GetImage(source, 3);
            source = $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/item/{summonerPlayer.Item1}.png";
            var itemImage1 = Methods.GetImage(source, 3);
            source = $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/item/{summonerPlayer.Item2}.png";
            var itemImage2 = Methods.GetImage(source, 3);
            source = $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/item/{summonerPlayer.Item3}.png";
            var itemImage3 = Methods.GetImage(source, 3);
            source = $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/item/{summonerPlayer.Item4}.png";
            var itemImage4 = Methods.GetImage(source, 3);
            source = $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/item/{summonerPlayer.Item5}.png";
            var itemImage5 = Methods.GetImage(source, 3);
            source = $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/item/{summonerPlayer.Item6}.png";
            var itemImage6 = Methods.GetImage(source, 3);

            Grid.SetColumn(itemImage0, 0);
            Grid.SetRow(itemImage0, 2);
            gameItemsGrid.Children.Add(itemImage0);
            Grid.SetColumn(itemImage1, 1);
            Grid.SetRow(itemImage0, 2);
            gameItemsGrid.Children.Add(itemImage1);
            Grid.SetColumn(itemImage2, 2);
            Grid.SetRow(itemImage2, 2);
            gameItemsGrid.Children.Add(itemImage2);
            Grid.SetColumn(itemImage3, 3);
            Grid.SetRow(itemImage3, 2);
            gameItemsGrid.Children.Add(itemImage3);
            Grid.SetColumn(itemImage4, 4);
            Grid.SetRow(itemImage4, 2);
            gameItemsGrid.Children.Add(itemImage4);
            Grid.SetColumn(itemImage5, 5);
            Grid.SetRow(itemImage5, 2);
            gameItemsGrid.Children.Add(itemImage5);
            Grid.SetColumn(itemImage6, 5);
            Grid.SetRow(itemImage6, 2);
            gameItemsGrid.Children.Add(itemImage6);

            source =
                $"http://ddragon.leagueoflegends.com/cdn/14.2.1/img/champion/{summonerPlayer.ChampionName}.png";
            var championIcon = Methods.GetImage(source, 10, 50);

            Grid.SetColumn(championIcon, 0);
            Grid.SetRowSpan(championIcon, 2);
            Grid.SetRow(championIcon, 0);
            Grid.SetColumnSpan(championIcon, 2);
            gameItemsGrid.Children.Add(championIcon);

            var perksJson =
                File.ReadAllText(
                    @"C:\Users\alcam\OneDrive\Developpement\nexus-client\NexusClient\NexusClient\Assets\loldata\14.1.1\data\fr_FR\runesReforged.json");
            var runesClass = JsonConvert.DeserializeObject<List<PerksClass.Root>>(perksJson);

            var firstPerkIcon = "";
            var secondPerkIcon = "";

            foreach (var root in runesClass)
            {
                if (root.id == summonerPlayer.Perks.Styles[0].Style)
                    foreach (var rune in root.slots[0].runes)
                        if (rune.id == summonerPlayer.Perks.Styles[0].Selections[0].Perk)
                            firstPerkIcon = rune.icon;

                if (root.id == summonerPlayer.Perks.Styles[1].Style) secondPerkIcon = root.icon;
            }


            var mainRuneUrl =
                $"https://ddragon.leagueoflegends.com/cdn/img/{firstPerkIcon}";

            var mainRune = new Image
            {
                Source = new BitmapImage(new Uri(mainRuneUrl))
            };

            Grid.SetColumn(mainRune, 2);
            Grid.SetRow(mainRune, 0);
            gameItemsGrid.Children.Add(mainRune);

            var secondaryRune = new Image
            {
                Source = new BitmapImage(new Uri(
                    $"https://ddragon.leagueoflegends.com/cdn/img/{secondPerkIcon}"))
            };

            Grid.SetColumn(secondaryRune, 2);
            Grid.SetRow(secondaryRune, 1);
            gameItemsGrid.Children.Add(secondaryRune);


            Grid.SetColumn(matchGrid, i);
            MatchListGrid.Children.Add(matchGrid);
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


    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.GoBack(new DrillInNavigationTransitionInfo());
    }
}