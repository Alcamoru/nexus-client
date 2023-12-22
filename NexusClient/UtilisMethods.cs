using System;
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace NexusClient;

public class UtilisMethods
{
    public static Border GetImage(string url, int cornerRadius = 0, int width = 0)
    {
        Image image = new Image()
        {
            Source = new BitmapImage(new Uri(url)),
        };
        Border border = new Border()
        {
            Child = image,
            CornerRadius = new CornerRadius(cornerRadius)
        };

        if (width != 0)
        {
            image.Width = width;
            image.Height = width;
            border.Width = width;
            border.Height = width;
        }

        return border;
    }

    public static Viewbox SetText(string text, int fontSize,
        Color color,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment verticalAlignment = VerticalAlignment.Center,
        Stretch stretch = Stretch.None)
    {

        TextBlock textBlock = new TextBlock()
        {
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            Foreground = new SolidColorBrush(color),
            TextAlignment = TextAlignment.Center,
            Text = text,
            FontSize = fontSize,
            FontFamily = new FontFamily("Assets/fonts/Inter/Inter-Medium.ttf#Inter")
        };

        Viewbox viewbox = new Viewbox()
        {
            Stretch = stretch,
            Child = textBlock
        };
        return viewbox;
    }
}