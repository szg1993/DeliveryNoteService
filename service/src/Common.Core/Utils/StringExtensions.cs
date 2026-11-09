using Common.Core.Configuration;
using Common.Core.Exceptions;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Common.Core.Utils;

public static class StringExtensions
{
    public static string ToCamelCase(this string value)
        => value.Length == 0 ? "" : value[..1].ToLower() + value[1..];

    public static DateOnly ParseDateOnly(this string dateString)
    {
        if (!TryParseDateOnly(dateString, out var date))
            throw new DataValidationException(CommonErrorCodes.InvalidDateFormat);

        return date;
    }

    public static bool TryParseDateOnly(this string? dateString, out DateOnly date)
    {
        return DateOnly.TryParse(dateString?.Trim(), CultureInfo.GetCultureInfo("hu-HU"), out date);
    }

    public static bool TryParseDateTimeOffset(this string? dateTimeOffsetString, out DateTimeOffset dateTimeOffset)
        => DateTimeOffset.TryParse(dateTimeOffsetString?.Trim(), CultureInfo.GetCultureInfo("hu-HU"), out dateTimeOffset);

    public static string SerializeToJson(this object value)
        => JsonSerializer.Serialize(value, JsonConfiguration.GetOptions());

    public static T? DeserializeJson<T>(this string json)
        => JsonSerializer.Deserialize<T>(json, JsonConfiguration.GetOptions());

    public static string EncodeToBase64(this string value)
    {
        return string.IsNullOrEmpty(value)
           ? value
           : Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    }

    public static string DecodeFromBase64(this string value)
    {
        return string.IsNullOrEmpty(value)
           ? value
           : Encoding.UTF8.GetString(Convert.FromBase64String(value));
    }

    public static void ValidateMaxLength(this string? self, int maxLength, string fieldName)
    {
        if (!self.IsNullOrEmpty() && self?.Length > maxLength)
            throw new FieldTooLongException(fieldName, maxLength);
    }
}