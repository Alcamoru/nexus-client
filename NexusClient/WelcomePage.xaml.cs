using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.AccountV1;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NexusClient;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class WelcomePage : Page
{
    public WelcomePage()
    {
        SummonerRegionalRoute = RegionalRoute.EUROPE;
        SummonerPlatformRoute = PlatformRoute.EUW1;
        var sr =
            new StreamReader(
                @"C:\Users\alcam\OneDrive\Developpement\nexus-client\NexusClient\NexusClient\RIOT_TOKEN.txt");
        var token = sr.ReadLine();
        InitializeComponent();
        Api = RiotGamesApi.NewInstance(token!);
        Debug.WriteLine(Directory.GetCurrentDirectory());
        // var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        // if (localSettings.Values.ContainsKey("SummonerName") & localSettings.Values.ContainsKey("RiotID"))
        // {
        //     SummonerNameTextBox.Text = localSettings.Values["SummonerName"].ToString();
        //     AccountTextBox.Text = localSettings.Values["RiotID"].ToString();
        //     CheckIfExists();
        // }
    }

    private RiotGamesApi Api { get; }
    private Summoner LolSummoner { get; set; }

    private RegionalRoute SummonerRegionalRoute { get; set; }

    private PlatformRoute SummonerPlatformRoute { get; set; }


    // Pour sélectionner la région
    private void RegionListButton_OnClick(object sender, RoutedEventArgs e)
    {
        RegionListView.Visibility = RegionListView.Visibility == Visibility.Collapsed
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void RegionListButton_OnLostFocus(object sender, RoutedEventArgs e)
    {
        RegionListView.Visibility = Visibility.Collapsed;
    }

    private void RegionListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = (ListViewItem)e.AddedItems[0];
        var itemContent = (TextBlock)item.Content;
        RegionListButtonTextBlock.Text = itemContent.Text;
        switch (itemContent.Text)
        {
            case "EUW":
                SummonerRegionalRoute = RegionalRoute.EUROPE;
                SummonerPlatformRoute = PlatformRoute.EUW1;
                break;
            case "NA":
                SummonerRegionalRoute = RegionalRoute.AMERICAS;
                SummonerPlatformRoute = PlatformRoute.NA1;
                break;
        }
    }

    private void SendSummonerNameButton_OnClick(object sender, RoutedEventArgs e)
    {
        CheckIfExists();
    }

    private void SummonerNameTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) CheckIfExists();
    }

    private async void CheckIfExists()
    {
        WelcomePageProgressRing.IsActive = true;

        try
        {
            Account lolAccount = (await Api.AccountV1().GetByRiotIdAsync(SummonerRegionalRoute, SummonerNameTextBox.Text, AccountTextBox.Text))!;
            LolSummoner = await Api.SummonerV4().GetByPUUIDAsync(SummonerPlatformRoute, lolAccount.Puuid);

            if (LolSummoner is null) throw new ArgumentNullException();

            await Task.Run(() => { Thread.Sleep(1); });
            // var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            // localSettings.Values["SummonerName"] = SummonerNameTextBox.Text;
            // localSettings.Values["RiotID"] = AccountTextBox.Text;

            NavigateToSummonerInfoPage();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            if (e is ArgumentNullException) ErrorTextBlock.Text = "L'invocateur recherché est invalide";

            if (e is AggregateException) ErrorTextBlock.Text = "Le logiciel n'est pas connecté à Internet";


            WelcomePageProgressRing.IsActive = false;
        }
    }

    private void NavigateToSummonerInfoPage()
    {
        var parametersList = new List<object>
        {
            Api,
            LolSummoner,
            SummonerRegionalRoute,
            SummonerPlatformRoute
        };

        Frame.Navigate(typeof(SummonerInfoPage), parametersList, new DrillInNavigationTransitionInfo());
    }
}