using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        InitializeComponent();
    }

    private void RegionListButton_OnClick(object sender, RoutedEventArgs e)
    {
        RegionListView.Visibility = RegionListView.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    private void RegionListButton_OnLostFocus(object sender, RoutedEventArgs e)
    {
        RegionListView.Visibility = Visibility.Collapsed;
    }

    private void RegionListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ListViewItem item = (ListViewItem)e.AddedItems[0];
        Debug.WriteLine(item.Name);
        RegionListButtonTextBlock.Text = item.Name.Substring(0, 3);
    }
}