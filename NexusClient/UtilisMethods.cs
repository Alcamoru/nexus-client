using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using Windows.UI;
using Windows.UI.Text;
using Camille.RiotGames;
using Camille.RiotGames.MatchV5;
using Camille.RiotGames.SummonerV4;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using static NexusClient.SummonerNamePage;

namespace NexusClient;

public static class UtilisMethods
{
    public static List<Match> GetLastMatches(string summonerPuuid, int count)
    {
        var matches = new List<Match>();
        var matchListIds = Api.MatchV5().GetMatchIdsByPUUID(SummonerRegionalRoute, summonerPuuid, count);
        foreach (var matchListId in matchListIds)
            matches.Add(Api.MatchV5().GetMatch(SummonerRegionalRoute, matchListId));
        return matches;
    }

    public static List<Image> GetPerks(int firstStyleId, int firstPerkId, int secondStyleId)
    {
        var perksJson =
            File.ReadAllText(
                @"C:\Users\alcam\OneDrive\Bureau\nexus-client\NexusClient\NexusClient\Assets\loldata\14.1.1\data\fr_FR\runesReforged.json");
        var perksReforgedNode = JsonNode.Parse(perksJson);

        var firstPerkUrl = "";
        var secondPerkUrl = "";
        foreach (var perk in perksReforgedNode!.AsArray())
        {
            if ((int)perk["id"] == firstStyleId)
                foreach (var mainPerk in perk!["slots"]![0]!["runes"]!.AsArray())
                    if ((int)mainPerk["id"] == firstPerkId)
                        firstPerkUrl = mainPerk["icon"]!.ToString();

            if ((int)perk["id"] == secondStyleId)
                secondPerkUrl = perk["icon"]!.ToString();
        }


        var mainRuneUrl =
            $"https://ddragon.leagueoflegends.com/cdn/img/{firstPerkUrl}";

        var mainPerkImage = new Image
        {
            Source = new BitmapImage(new Uri(mainRuneUrl))
        };


        var secondPerkImage = new Image
        {
            Source = new BitmapImage(new Uri(
                $"https://ddragon.leagueoflegends.com/cdn/img/{secondPerkUrl}"))
        };

        return new List<Image> { mainPerkImage, secondPerkImage };
    }

    public static string GetSummonerName(string summonerId)
    {
        var summoner = Api.SummonerV4().GetBySummonerId(SummonerPlatformRoute, summonerId);
        return Api.AccountV1().GetByPuuid(SummonerRegionalRoute, summoner.Puuid).GameName;
    }

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

    public static Border GetProfileIcon(string summonerId, int cornerRadius = 0, int width = 0)
    {
        var url =
            $@"ms-appx:///Assets/loldata/14.1.1/img/profileicon/{Api.SummonerV4().GetBySummonerId(SummonerPlatformRoute, summonerId)!.ProfileIconId}.png";
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

    public static Border GetChampionImage(string championName, int cornerRadius = 0, int width = 0)
    {
        var url = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/champion/{championName}.png";
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


    public static Border GetSummonerSpellImage(int summonerId, int cornerRadius = 0, int width = 0)
    {
        var sumsCorrespondences = new Dictionary<int, string>
        {
            { 21, "SummonerBarrier" },
            { 1, "SummonerBoost" },
            { 2202, "SummonerCherryFlash" },
            { 2201, "SummonerCherryHold" },
            { 14, "SummonerDot" },
            { 3, "SummonerExhaust" },
            { 4, "SummonerFlash" },
            { 6, "SummonerHaste" },
            { 7, "SummonerHeal" },
            { 13, "SummonerMana" },
            { 30, "SummonerPoroRecall" },
            { 31, "SummonerPoroThrow" },
            { 11, "SummonerSmite" },
            { 39, "SummonerSnowURFSnowball_Mark" },
            { 32, "SummonerSnowball" },
            { 12, "SummonerTeleport" },
            { 54, "Summoner_UltBookPlaceholder" },
            { 55, "Summoner_UltBookSmitePlaceholder" }
        };

        var url = $"http://ddragon.leagueoflegends.com/cdn/14.8.1/img/spell/{sumsCorrespondences[summonerId]}.png";
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
    /// <param name="fontWeight"></param>
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
    public static Viewbox SetTextWithViewbox(string text, int fontSize,
        Color color,
        int fontWeight = 400,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment verticalAlignment = VerticalAlignment.Center)
    {
        var textBlock = new TextBlock
        {
            FontWeight = new FontWeight((ushort)fontWeight),
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            Foreground = new SolidColorBrush(color),
            TextAlignment = TextAlignment.Center,
            Text = text,
            FontSize = fontSize
        };

        return new Viewbox()
        {
            Child = textBlock
        };
    }
    public static TextBlock SetText(string text, int fontSize,
        Color color,
        int fontWeight = 400,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment verticalAlignment = VerticalAlignment.Center)
    {
        var textBlock = new TextBlock
        {
            FontWeight = new FontWeight((ushort)fontWeight),
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            Foreground = new SolidColorBrush(color),
            TextAlignment = TextAlignment.Center,
            Text = text,
            FontSize = fontSize
        };
        return textBlock;
    }
}