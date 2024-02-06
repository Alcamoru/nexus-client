using System;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace NexusClient;

public class UtilisMethods
{
    /// <summary>
    ///     Retrieves an image from a given URL and wraps it in a border with optional corner radius and width.
    /// </summary>
    /// <param name="url">The URL of the image to be retrieved.</param>
    /// <param name="cornerRadius">The corner radius to be applied to the border. Defaults to 0.</param>
    /// <param name="width">The desired width of the image and the border. Defaults to 0.</param>
    /// <returns>
    ///     The retrieved image wrapped in a border with applied corner radius and width, if specified.
    /// </returns>
    public static Border GetImage(string url, int cornerRadius = 0, int width = 0)
    {
        var image = new Image
        {
            Source = new BitmapImage(new Uri(url))
        };
        var border = new Border
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

    /// <summary>
    ///     Creates a Viewbox element with a TextBlock child that displays the specified text.
    /// </summary>
    /// <param name="text">The text to be displayed.</param>
    /// <param name="fontSize">The font size of the displayed text.</param>
    /// <param name="color">The color of the displayed text.</param>
    /// <param name="horizontalAlignment">
    ///     The horizontal alignment of the TextBlock within the Viewbox. Defaults to
    ///     HorizontalAlignment.Center.
    /// </param>
    /// <param name="verticalAlignment">
    ///     The vertical alignment of the TextBlock within the Viewbox. Defaults to
    ///     VerticalAlignment.Center.
    /// </param>
    /// <param name="stretch">The stretch mode for the Viewbox. Defaults to Stretch.None.</param>
    /// <returns>A Viewbox element with a TextBlock child that displays the specified text.</returns>
    public static Viewbox SetText(string text, int fontSize,
        Color color,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment verticalAlignment = VerticalAlignment.Center,
        Stretch stretch = Stretch.None)
    {
        var textBlock = new TextBlock
        {
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            Foreground = new SolidColorBrush(color),
            TextAlignment = TextAlignment.Center,
            Text = text,
            FontSize = fontSize,
        };

        var viewbox = new Viewbox
        {
            Stretch = stretch,
            Child = textBlock
        };
        return viewbox;
    }
}