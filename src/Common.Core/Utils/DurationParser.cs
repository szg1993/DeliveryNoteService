using System.Text.RegularExpressions;

namespace Medusa.Enap.Common.Core.Utils;

public static partial class DurationParser
{
    private static readonly Regex _durationRegex =
        GeneratedRegex();

    public static TimeSpan Parse(string input)
    {
        Validate(input);

        TimeSpan result = TimeSpan.Zero;

        foreach (Match match in _durationRegex.Matches(input))
            result += ParseMatch(match);

        return result;
    }

    private static void Validate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Duration string is empty.");

        if (!_durationRegex.IsMatch(input))
            throw new FormatException("Invalid duration format.");
    }

    private static TimeSpan ParseMatch(Match match)
    {
        int value = int.Parse(match.Groups[1].Value);
        char unit = char.ToLower(match.Groups[2].Value[0]);

        return ToTimeSpan(value, unit);
    }

    private static TimeSpan ToTimeSpan(int value, char unit) =>
        unit switch
        {
            's' => TimeSpan.FromSeconds(value),
            'm' => TimeSpan.FromMinutes(value),
            'h' => TimeSpan.FromHours(value),
            'd' => TimeSpan.FromDays(value),
            _ => throw new FormatException($"Unknown unit '{unit}'.")
        };

    [GeneratedRegex(@"(\d+)([smhd])", RegexOptions.IgnoreCase, "hu-HU")]
    private static partial Regex GeneratedRegex();
}