using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia.Media;

namespace Shoebill.Helpers;

public class AnsiTextSegment
{
    public required string Text { get; set; }
    public required IBrush Foreground { get; set; }
}

public static partial class AnsiHelper
{
    private static readonly Dictionary<int, IBrush> AnsiColors = new()
    {
        { 0, Brushes.White },
        { 30, Brushes.Black },
        { 31, Brushes.DarkRed },
        { 32, Brushes.Green },
        { 33, Brushes.Yellow },
        { 34, Brushes.Blue },
        { 35, Brushes.DarkMagenta },
        { 36, Brushes.DarkCyan },
        { 37, Brushes.LightGray },
        { 90, Brushes.DimGray },
        { 91, Brushes.Red },
        { 92, Brushes.LawnGreen },
        { 93, Brushes.LightYellow },
        { 94, Brushes.RoyalBlue },
        { 95, Brushes.Magenta },
        { 96, Brushes.Cyan },
        { 97, Brushes.White }
    };

    private static readonly Regex AnsiRegex = CAnsiRegex();

    public static List<AnsiTextSegment> Parse(string text)
    {
        var segments = new List<AnsiTextSegment>();
        var matches = AnsiRegex.Matches(text);

        var lastIndex = 0;
        IBrush currentForeground = Brushes.White;

        foreach (Match match in matches)
        {
            var startIndex = match.Index;
            if (startIndex > lastIndex)
                segments.Add(new AnsiTextSegment
                    { Text = text[lastIndex..startIndex], Foreground = currentForeground });

            if (int.TryParse(match.Groups[1].Value, out var colorCode) &&
                AnsiColors.TryGetValue(colorCode, out var color))
                currentForeground = color;

            lastIndex = startIndex + match.Length;
        }

        if (lastIndex < text.Length)
            segments.Add(new AnsiTextSegment { Text = text[lastIndex..], Foreground = currentForeground });

        return segments;
    }

    [GeneratedRegex(@"\x1b\[(\d+)m", RegexOptions.Compiled)]
    private static partial Regex CAnsiRegex();
}