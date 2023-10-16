using System.Diagnostics;
using Windows.System;
using Camille.RiotGames;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NexusClient;

/// <summary>
///     An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class WelcomePage : Page
{
    public RiotGamesApi RiotGamesApi = RiotGamesApi.NewInstance("");

    public WelcomePage()
    {
        InitializeComponent();
    }

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
        Debug.WriteLine(item.Name);
        RegionListButtonTextBlock.Text = item.Name.Substring(0, 3);
    }

    private void SendSummonerNameButton_OnClick(object sender, RoutedEventArgs e)
    {
        var summonerName = SummonerNameTextBox.Text;
        if (summonerName.Length != 0) Frame.Navigate(typeof(SummonerInfoPage), summonerName);
    }

    private void SummonerNameTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            var summonerName = SummonerNameTextBox.Text;
            if (summonerName.Length != 0) Frame.Navigate(typeof(SummonerInfoPage), summonerName);
        }
    }
}