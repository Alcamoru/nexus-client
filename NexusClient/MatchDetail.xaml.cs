using System;
using System.Collections.Generic;
using System.Linq;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI;
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
public sealed partial class MatchDetail : Page
{
    public MatchDetail()
    {
        InitializeComponent();
    }

    private RiotGamesApi Api { get; set; }

    private Match MatchInfo { get; set; }
    private Summoner LolSummoner { get; set; }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not List<object> parameters) return;
        Api = (RiotGamesApi)parameters.ElementAt(0);
        MatchInfo = (Match)parameters.ElementAt(1);
        LolSummoner = (Summoner)parameters.ElementAt(2);
        base.OnNavigatedTo(e);
    }

    private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
    {
        Frame.GoBack();
    }

    private void SetMatchTimeline()
    {
        MatchTimeline timeline = Api.MatchV5().GetTimeline(RegionalRoute.EUROPE, MatchInfo.Metadata.MatchId);
        int summonerParticipantId = 0;
        foreach (var participant in timeline!.Info.Participants!)
        {
            if (participant.Puuid == LolSummoner.Puuid)
            {
                summonerParticipantId = participant.ParticipantId;
            };
        }


        Image wardImage = new Image()
        {
            Width = 40,
            Source = new BitmapImage(new Uri(
                "https://static.wikia.nocookie.net/leagueoflegends/images/e/e2/Warding_Totem_item.png/revision/latest/smart/width/250/height/250?cb=20201109132946"))
        };

        TextBlock wardPlacedTextBlock = new TextBlock()
        {
            Foreground = new SolidColorBrush(Colors.White),
            HorizontalTextAlignment = TextAlignment.Center,
            Text = " a placé une balise",
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };

        foreach (MatchTimelineInfoFrame timelineInfoFrame in timeline.Info.Frames)
        {
            foreach (MatchTimelineInfoFrameEvent e in timelineInfoFrame.Events)
            {
                if (e.Type == "WARD_PLACED" && e.ParticipantId == summonerParticipantId)
                {
                    StackPanel FrameInfoStackPanel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal
                    };
                    FrameInfoStackPanel.Children.Add(wardImage);
                    FrameInfoStackPanel.Children.Add(wardPlacedTextBlock);
                    Grid.SetColumn(FrameInfoStackPanel,1);

                    MatchTimelineGrid.Children.Add(FrameInfoStackPanel);
                }
            }
        }
    }
}