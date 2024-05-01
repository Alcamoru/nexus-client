using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Camille.Enums;
using Camille.RiotGames;
using Camille.RiotGames.SummonerV4;
using Camille.RiotGames.Util;
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
public sealed partial class SummonerName : Page
{
    public SummonerName()
    {
        SummonerRegionalRoute = RegionalRoute.EUROPE;
        SummonerPlatformRoute = PlatformRoute.EUW1;
        var sr =
            new StreamReader(
                @"C:\Users\alcam\OneDrive\Bureau\nexus-client\NexusClient\NexusClient\RIOT_TOKEN.txt");
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

    internal static RiotGamesApi Api { get; set; }
    internal static Summoner LolSummoner { get; set; }

    internal static RegionalRoute SummonerRegionalRoute { get; set; }

    internal static PlatformRoute SummonerPlatformRoute { get; set; }


    private void SummonerNameTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) CheckIfExists();
    }

    private async void CheckIfExists()
    {
        WelcomePageProgressRing.IsActive = true;

        try
        {
            var lolAccount = (await Api.AccountV1()
                .GetByRiotIdAsync(SummonerRegionalRoute, SummonerNameAutoSuggestBox.Text, RiotIdAutoSuggestBox.Text))!;
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
            if (e is ArgumentNullException || e is RiotResponseException)
                ErrorTextBlock.Text = "L'invocateur recherché est invalide";

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

        Frame.Navigate(typeof(MainPage), parametersList, new DrillInNavigationTransitionInfo());
    }

    private void MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
    {
        var buttonClicked = (MenuFlyoutItem)e.OriginalSource;
        switch (buttonClicked.Text)
        {
            case "EUW":
                SummonerRegionalRoute = RegionalRoute.EUROPE;
                SummonerPlatformRoute = PlatformRoute.EUW1;
                DropDownButton.Content = "EUW";
                break;
            case "NA":
                SummonerRegionalRoute = RegionalRoute.AMERICAS;
                SummonerPlatformRoute = PlatformRoute.NA1;
                DropDownButton.Content = "NA";

                break;
        }
    }

    private void ButtonSearch_OnClick(object sender, RoutedEventArgs e)
    {
        CheckIfExists();
    }
}