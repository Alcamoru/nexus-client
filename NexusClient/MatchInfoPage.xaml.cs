using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
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
using SkiaSharp;
using Team = Camille.RiotGames.Enums.Team;
using static NexusClient.SummonerNamePage;
using static NexusClient.UtilisMethods;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NexusClient;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MatchInfoPage : Page
{
    public MatchInfoPage()
    {
        InitializeComponent();
    }

    private Match MatchInfo { get; set; }

    private List<Participant> BlueTeam { get; set; }

    private List<Participant> RedTeam { get; set; }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        MatchInfo = (Match)e.Parameter;
        BlueTeam = new List<Participant>();
        RedTeam = new List<Participant>();
        SetTeams();
        SetMatchTimeline();
        SetParticipantsGrid();
        SetMatchGraph();
        base.OnNavigatedTo(e);
    }


    private string GetChampionNameByPuuid(string puuid)
    {
        var championName = "";
        foreach (var participant in MatchInfo.Info.Participants)
            if (participant.Puuid == puuid)
                championName = participant.ChampionName;

        return championName;
    }

    /// <summary>
    /// Sets up the participants grid in the MatchInfoPage.
    /// </summary>
    private void SetParticipantsGrid()
    {
        foreach (var participant in MatchInfo.Info.Participants)
            if (GetSummonerName(participant.SummonerId) == GetSummonerName(LolSummoner.Id))
            {
                if (participant.Win)
                {
                    SummonerWonTextBlock.Text = "Victoire";
                    SummonerWonTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96));
                }
                else
                {
                    SummonerWonTextBlock.Text = "Défaite";
                    SummonerWonTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6));
                }
            }

        var row = 0;

        var blueTeamBestDamages = 0;
        foreach (var participant in BlueTeam)
            if (participant.TotalDamageDealtToChampions > blueTeamBestDamages)
                blueTeamBestDamages = participant.TotalDamageDealtToChampions;
        var redTeamBestDamages = 0;
        foreach (var participant in RedTeam)
            if (participant.TotalDamageDealtToChampions > redTeamBestDamages)
                redTeamBestDamages = participant.TotalDamageDealtToChampions;

        foreach (var participant in BlueTeam)
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
                Padding = new Thickness(5),
                Tag = participant.SummonerId
            };

            if (participant.SummonerId == LolSummoner.Id)
                participantGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96));
            else
                participantGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 41, 128, 185));

            participantGrid.BorderThickness = new Thickness(2);

            for (var i = 0; i < 6; i++)
                if (i == 1)
                    participantGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(2, GridUnitType.Star) });
                else
                    participantGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(1, GridUnitType.Star) });


            participantGrid.PointerEntered += (_, _) =>
                ProtectedCursor = InputCursor.CreateFromCoreCursor(new CoreCursor(CoreCursorType.Hand, 1));
            participantGrid.PointerExited += (_, _) =>
                ProtectedCursor = InputCursor.CreateFromCoreCursor(new CoreCursor(CoreCursorType.Arrow, 1));
            participantGrid.PointerPressed += ChampionIconViewboxOnPointerPressed;

            var championIconViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = GetChampionImage(participant.ChampionName, 30, 15)
            };

            var summonerNameViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = SetText(GetSummonerName(participant.SummonerId),
                    20, Color.FromArgb(255, 52, 73, 94))
            };

            Grid.SetColumn(championIconViewbox, 0);
            participantGrid.Children.Add(championIconViewbox);

            Grid.SetColumn(summonerNameViewbox, 1);
            participantGrid.Children.Add(summonerNameViewbox);

            var summonersViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var summonerChampionGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            for (var i = 0; i < 2; i++)
                summonerChampionGrid.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = new GridLength(15, GridUnitType.Pixel) });

            for (var i = 0; i < 2; i++)
                summonerChampionGrid.RowDefinitions.Add(new RowDefinition
                    { Height = new GridLength(15, GridUnitType.Pixel) });

            var firstSummonerSpellImage = GetSummonerSpellImage(participant.Summoner1Id);

            Grid.SetColumn(firstSummonerSpellImage, 0);
            Grid.SetRow(firstSummonerSpellImage, 0);
            summonerChampionGrid.Children.Add(firstSummonerSpellImage);

            var secondSummonerSpellImage = GetSummonerSpellImage(participant.Summoner2Id);

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

            summonersViewbox.Child = summonerChampionGrid;

            Grid.SetColumn(summonersViewbox, 2);
            participantGrid.Children.Add(summonersViewbox);

            var csChampionTextBlock = SetText(
                $"{participant.TotalMinionsKilled + participant.TotalAllyJungleMinionsKilled + participant.TotalEnemyJungleMinionsKilled} CS",
                15, Color.FromArgb(255, 52, 73, 94), stretch: Stretch.Uniform);

            Grid.SetColumn(csChampionTextBlock, 3);
            participantGrid.Children.Add(csChampionTextBlock);

            var blueTeamBorder = new Border
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new Rectangle
                {
                    Width = 50,
                    Fill = new SolidColorBrush(Colors.White),
                    Height = 7
                },
                CornerRadius = new CornerRadius(3)
            };


            var width = participant.TotalDamageDealtToChampions / (float)blueTeamBestDamages * 50;

            var participantBorder = new Border
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = width,
                    Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
                    Height = 7
                },
                CornerRadius = new CornerRadius(3)
            };

            var innerGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { blueTeamBorder, participantBorder }
            };

            Grid.SetColumn(innerGrid, 4);
            participantGrid.Children.Add(innerGrid);

            var kdaChampionTextBlock = SetText(
                $"{participant.Kills} | {participant.Deaths} | {participant.Assists}",
                15, Color.FromArgb(255, 52, 73, 94), stretch: Stretch.Uniform);

            Grid.SetColumn(kdaChampionTextBlock, 5);
            participantGrid.Children.Add(kdaChampionTextBlock);

            Grid.SetColumn(participantGrid, 0);
            Grid.SetRow(participantGrid, row);
            ParticipantsGrid.Children.Add(participantGrid);
        }

        foreach (var participant in RedTeam)
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

            if (participant.SummonerId == LolSummoner.Id)
                participantGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96));
            else
                participantGrid.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 235, 47, 6));

            participantGrid.BorderThickness = new Thickness(2);
            for (var i = 0; i < 6; i++)
                if (i == 1)
                    participantGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(2, GridUnitType.Star) });
                else
                    participantGrid.ColumnDefinitions.Add(new ColumnDefinition
                        { Width = new GridLength(1, GridUnitType.Star) });

            var championIconViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = GetChampionImage(participant.ChampionName, 30, 15)
            };

            var championNameViewbox = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 52, 73, 94)),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = GetSummonerName(participant.SummonerId),
                    FontSize = 20,
                    FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
                }
            };

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

            for (var i = 0; i < 2; i++)
                summonerChampionGrid.ColumnDefinitions.Add(new ColumnDefinition
                    { Width = new GridLength(15, GridUnitType.Pixel) });

            for (var i = 0; i < 2; i++)
                summonerChampionGrid.RowDefinitions.Add(new RowDefinition
                    { Height = new GridLength(15, GridUnitType.Pixel) });

            var firstSummonerSpellImage = GetSummonerSpellImage(participant.Summoner1Id);

            Grid.SetColumn(firstSummonerSpellImage, 0);
            Grid.SetRow(firstSummonerSpellImage, 0);
            summonerChampionGrid.Children.Add(firstSummonerSpellImage);

            var secondSummonerSpellImage = GetSummonerSpellImage(participant.Summoner2Id);

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

            summonersViewbox.Child = summonerChampionGrid;

            Grid.SetColumn(summonersViewbox, 2);
            participantGrid.Children.Add(summonersViewbox);

            var redTeamBorder = new Border
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Child = new Rectangle
                {
                    Width = 50,
                    Fill = new SolidColorBrush(Colors.White),
                    Height = 7
                },
                CornerRadius = new CornerRadius(3)
            };


            var width = participant.TotalDamageDealtToChampions / (float)redTeamBestDamages * 50;

            var participantBorder = new Border
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = width,
                    Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
                    Height = 7
                },
                CornerRadius = new CornerRadius(3)
            };

            var innerGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children = { redTeamBorder, participantBorder }
            };

            Grid.SetColumn(innerGrid, 4);
            participantGrid.Children.Add(innerGrid);

            var csChampionTextBlock = SetText(
                $"{participant.TotalMinionsKilled + participant.TotalAllyJungleMinionsKilled + participant.TotalEnemyJungleMinionsKilled} CS",
                15, Color.FromArgb(255, 52, 73, 94), stretch: Stretch.Uniform);


            Grid.SetColumn(csChampionTextBlock, 3);
            participantGrid.Children.Add(csChampionTextBlock);

            var kdaChampionTextBlock = SetText(
                $"{participant.Kills} | {participant.Deaths} | {participant.Assists}",
                15, Color.FromArgb(255, 52, 73, 94), stretch: Stretch.Uniform);

            Grid.SetColumn(kdaChampionTextBlock, 5);
            participantGrid.Children.Add(kdaChampionTextBlock);


            Grid.SetColumn(participantGrid, 2);
            Grid.SetRow(participantGrid, row);
            ParticipantsGrid.Children.Add(participantGrid);
        }
    }

    private async void ChampionIconViewboxOnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var originalSource = e.OriginalSource as FrameworkElement;

        Grid player = null;

        while (originalSource != null)
        {
            if (originalSource is Grid)
            {
                player = originalSource as Grid;
                break;
            }

            originalSource = originalSource.Parent as FrameworkElement;
        }

        if (player != null)
        {
            var progressRing = new ProgressRing
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
            player!.Children.Add(matchRectangle);

            Grid.SetColumn(progressRing, 0);
            Grid.SetColumnSpan(progressRing, 6);
            player!.Children.Add(progressRing);
            await Task.Run(() => { Thread.Sleep(1); });

            Frame.Navigate(typeof(ProfilePage), (Summoner)player.Tag, new DrillInNavigationTransitionInfo());
        }
    }

    private void SetTeams()
    {
        foreach (var participant in MatchInfo.Info.Participants)
            if (participant.TeamId == Team.Blue)
                BlueTeam.Add(participant);
            else
                RedTeam.Add(participant);
    }


    /// <summary>
    /// Sets up the match timeline in the MatchInfoPage.
    /// </summary>
    private void SetMatchTimeline()
    {
        var elementNumber = 0;

        var precedentEventIsPositive = true;


        SetPositiveEvent();
        elementNumber += 1;

        var timeline = Api.MatchV5().GetTimeline(SummonerRegionalRoute, MatchInfo.Metadata.MatchId);
        var summonerParticipantId = 0;
        foreach (var participant in timeline!.Info.Participants!)
            if (participant.Puuid == LolSummoner.Puuid)
                summonerParticipantId = participant.ParticipantId;


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

        var startIcon = new Image
        {
            Width = 40,
            Source = new BitmapImage(
                new Uri("ms-appx:///Assets/media/bouton-de-lecture-video.png"))
        };

        var startTextBlock = SetText(" début du match",
            15, Colors.White);

        var startStackPanel = new StackPanel
        {
            Spacing = 10.0,
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10, 0, 0, 0),
            Children = { startIcon, startTextBlock }
        };
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

                var creatorPuuid = "";

                foreach (var participant in timeline.Info.Participants)
                    if (participant.ParticipantId == e.CreatorId)
                        creatorPuuid = participant.Puuid;

                var championIcon = GetChampionImage(GetChampionNameByPuuid(creatorPuuid), 7, 40);
                championIcon.VerticalAlignment = VerticalAlignment.Center;


                var source =
                    @"C:\Users\alcam\OneDrive\Bureau\Developpement\nexus-client\NexusClient\NexusClient\Assets\loldata\14.1.1\img\item\3340.png";
                var wardImage = GetImage(source, 7, 40);
                wardImage.VerticalAlignment = VerticalAlignment.Center;

                var wardPlacedTextBlock = SetText(" a placé une balise",
                    15, Colors.White);


                var frameInfoStackPanel = new StackPanel
                {
                    Spacing = 10.0,
                    Margin = new Thickness(10, 0, 0, 0),
                    Orientation = Orientation.Horizontal,
                    Children = { championIcon, wardPlacedTextBlock, wardImage }
                };
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

                    var killerPuuid = "";
                    var victimPuuid = "";

                    foreach (var participant in timeline.Info.Participants)
                    {
                        if (participant.ParticipantId == e.KillerId) killerPuuid = participant.Puuid;

                        if (participant.ParticipantId == e.VictimId) victimPuuid = participant.Puuid;
                    }

                    var killerChampionIcon = GetChampionImage(GetChampionNameByPuuid(killerPuuid), 7, 40);
                    killerChampionIcon.VerticalAlignment = VerticalAlignment.Center;

                    var victimChampionIcon = GetChampionImage(GetChampionNameByPuuid(victimPuuid), 7, 40);
                    victimChampionIcon.VerticalAlignment = VerticalAlignment.Center;

                    var eliminatedTextBlock = SetText(" a eliminé",
                        15, Colors.White);

                    var frameInfoStackPanel = new StackPanel
                    {
                        Spacing = 10,
                        Margin = new Thickness(10, 0, 0, 0),
                        Orientation = Orientation.Horizontal,
                        Children = { killerChampionIcon, eliminatedTextBlock, victimChampionIcon }
                    };
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

                    var killerPuuid = "";

                    foreach (var participant in timeline.Info.Participants)
                        if (participant.ParticipantId == e.KillerId)
                            killerPuuid = participant.Puuid;

                    var killerChampionIcon = GetChampionImage(GetChampionNameByPuuid(killerPuuid), 7, 40);
                    killerChampionIcon.VerticalAlignment = VerticalAlignment.Center;

                    var victimChampionIcon = GetChampionImage(GetChampionNameByPuuid(LolSummoner.Puuid), 7, 40);
                    victimChampionIcon.VerticalAlignment = VerticalAlignment.Center;

                    var eliminatedTextBlock = SetText(" a eliminé",
                        15, Colors.White);

                    var frameInfoStackPanel = new StackPanel
                    {
                        Spacing = 10,
                        Margin = new Thickness(10, 0, 0, 0),
                        Orientation = Orientation.Horizontal,
                        Children = { killerChampionIcon, eliminatedTextBlock, victimChampionIcon }
                    };
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

                    var victimPuuid = "";

                    foreach (var participant in timeline.Info.Participants)
                        if (participant.ParticipantId == e.VictimId)
                            victimPuuid = participant.Puuid;

                    var summonerChampionIcon =
                        GetChampionImage(GetChampionNameByPuuid(LolSummoner.Puuid), 7, 40);
                    summonerChampionIcon.VerticalAlignment = VerticalAlignment.Center;

                    var victimChampionIcon = GetChampionImage(GetChampionNameByPuuid(victimPuuid), 7, 40);
                    victimChampionIcon.VerticalAlignment = VerticalAlignment.Center;


                    var eliminatedTextBlock = SetText(" a participé à l'élimination de ",
                        15, Colors.White);

                    var frameInfoStackPanel = new StackPanel
                    {
                        Spacing = 10,
                        Margin = new Thickness(10, 0, 0, 0),
                        Orientation = Orientation.Horizontal,
                        Children = { summonerChampionIcon, eliminatedTextBlock, victimChampionIcon }
                    };
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

        var endStackPanel = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(10, 0, 0, 0),
            Orientation = Orientation.Horizontal
        };

        var endIcon = new Image
        {
            Width = 40,
            Source = new BitmapImage(
                new Uri("ms-appx:///Assets/media/bouton-de-lecture-video.png"))
        };

        var endTextBlock = SetText(" fin du match",
            15, Colors.White);

        endStackPanel.Children.Add(endIcon);
        endStackPanel.Children.Add(endTextBlock);
        Grid.SetColumn(endStackPanel, 1);
        Grid.SetRow(endStackPanel, elementNumber);

        var ellipseGreen = new Ellipse
        {
            Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
            Height = 10,
            Width = 10,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(ellipseGreen, 0);
        Grid.SetRow(ellipseGreen, elementNumber);
        MatchTimelineGrid.Children.Add(ellipseGreen);

        var rectangleGreenTop = new Rectangle
        {
            Fill = new SolidColorBrush(Color.FromArgb(255, 39, 174, 96)),
            Height = 37,
            Width = 4,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(rectangleGreenTop, 0);
        Grid.SetRow(rectangleGreenTop, elementNumber);
        MatchTimelineGrid.Children.Add(rectangleGreenTop);

        MatchTimelineGrid.Children.Add(endStackPanel);
    }


    /// <summary>
    /// Sets up the match graph in the MatchInfoPage.
    /// </summary>
    private void SetMatchGraph()
    {
        var timeline = Api.MatchV5().GetTimeline(SummonerRegionalRoute, MatchInfo.Metadata.MatchId)!;
        var blueTeamMoney = new ObservableCollection<int>();
        var redTeamMoney = new ObservableCollection<int>();

        var summonerGameId = 1;
        foreach (var metadataParticipant in timeline.Metadata.Participants)
        {
            if (metadataParticipant == LolSummoner.Puuid) break;

            summonerGameId++;
        }

        var playerMinions = new ObservableCollection<int>();
        var playerXp = new ObservableCollection<int>();

        foreach (var frame in timeline.Info.Frames)
        {
            var participantFrames =
                new List<MatchTimelineInfoFrameParticipantFrame>
                {
                    frame.ParticipantFrames!.X1,
                    frame.ParticipantFrames.X2,
                    frame.ParticipantFrames.X3,
                    frame.ParticipantFrames.X4,
                    frame.ParticipantFrames.X5,
                    frame.ParticipantFrames.X6,
                    frame.ParticipantFrames.X7,
                    frame.ParticipantFrames.X8,
                    frame.ParticipantFrames.X9,
                    frame.ParticipantFrames.X10
                };
            var totalBlueTeamMoney = 0;
            foreach (var participant in BlueTeam)
            foreach (var participantFrame in participantFrames)
                if (participant.ParticipantId == participantFrame.ParticipantId)
                    totalBlueTeamMoney += participantFrame.TotalGold;

            blueTeamMoney.Add(totalBlueTeamMoney);


            var totalRedTeamMoney = 0;
            foreach (var participant in RedTeam)
            foreach (var participantFrame in participantFrames)
                if (participant.ParticipantId == participantFrame.ParticipantId)
                    totalRedTeamMoney += participantFrame.TotalGold;

            redTeamMoney.Add(totalRedTeamMoney);

            foreach (var participantFrame in participantFrames)
                if (summonerGameId == participantFrame.ParticipantId)
                {
                    playerMinions.Add(participantFrame.MinionsKilled +
                                      participantFrame.JungleMinionsKilled);
                    playerXp.Add(frame.ParticipantFrames.X1.Xp);
                }
        }

        var blueTeamSerie = new LineSeries<int>
        {
            Values = blueTeamMoney,
            Stroke = new SolidColorPaint(new SKColor(41, 128, 185)),
            GeometrySize = 2
        };
        var redTeamSerie = new LineSeries<int>
        {
            Values = redTeamMoney,
            Stroke = new SolidColorPaint(new SKColor(235, 47, 6)),
            GeometrySize = 2
        };
        GoldChart.Series = new List<ISeries> { blueTeamSerie, redTeamSerie };

        var playerMinionsSeries = new LineSeries<int>
        {
            Values = playerMinions,
            Stroke = new SolidColorPaint(new SKColor(41, 128, 185)),
            GeometrySize = 2
        };
        MinionsChart.Series = new List<ISeries> { playerMinionsSeries };

        var playerXpSeries = new LineSeries<int>
        {
            Values = playerXp,
            Stroke = new SolidColorPaint(new SKColor(41, 128, 185)),
            GeometrySize = 2
        };
        XpChart.Series = new List<ISeries> { playerXpSeries };
    }

    private void GoldsChartButton_OnClick(object sender, RoutedEventArgs e)
    {
        MinionsChartButton.IsChecked = false;
        XpChartButton.IsChecked = false;
        GoldChart.Visibility = Visibility.Visible;
        XpChart.Visibility = Visibility.Collapsed;
        MinionsChart.Visibility = Visibility.Collapsed;
    }

    private void MinionsChartButton_OnClick(object sender, RoutedEventArgs e)
    {
        GoldsChartButton.IsChecked = false;
        XpChartButton.IsChecked = false;
        GoldChart.Visibility = Visibility.Collapsed;
        XpChart.Visibility = Visibility.Collapsed;
        MinionsChart.Visibility = Visibility.Visible;
    }

    private void XpChartButton_OnClick(object sender, RoutedEventArgs e)
    {
        GoldsChartButton.IsChecked = false;
        MinionsChartButton.IsChecked = false;
        GoldChart.Visibility = Visibility.Collapsed;
        MinionsChart.Visibility = Visibility.Collapsed;
        XpChart.Visibility = Visibility.Visible;
    }
}