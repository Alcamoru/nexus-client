using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace NexusClient;

public static class AppColors
{
    public static readonly LinearGradientBrush GoldGradient = new()
    {
        GradientStops = new GradientStopCollection
        {
            new() { Color = Color.FromArgb(255, 120, 90, 40) },
            new() { Color = Color.FromArgb(255, 200, 155, 60) }
        }
    };

    public static readonly SolidColorBrush GreyCool = new(Color.FromArgb(255, 30, 40, 45));
    public static readonly SolidColorBrush Blue4 = new(Color.FromArgb(255, 0, 90, 130));
    public static readonly SolidColorBrush Blue6 = new(Color.FromArgb(255, 9, 20, 40));
    public static readonly SolidColorBrush Gold3 = new(Color.FromArgb(255, 200, 170, 110));
    public static readonly SolidColorBrush Gold4 = new(Color.FromArgb(255, 200, 155, 60));
    public static readonly SolidColorBrush Gold5 = new(Color.FromArgb(255, 120, 90, 40));
    public static readonly SolidColorBrush Gold6 = new(Color.FromArgb(255, 70, 55, 20));
    public static readonly SolidColorBrush White = new(Colors.White);
}