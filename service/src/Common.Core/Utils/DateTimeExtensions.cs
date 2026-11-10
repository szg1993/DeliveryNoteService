namespace System;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo _hungarianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

    // UtcDateTime will ensure UTC, and the custom format will produce a value like 2024-06-13T12:18:28.735Z
    public static string ToIso8601String(this DateTimeOffset date) => date.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff'Z'");

    public static string ToIso8601String(this DateOnly date) => date.ToString("o");

    public static DateOnly ToDateOnly(this DateTimeOffset date)
        => DateOnly.FromDateTime(date.Date);

    public static TimeOnly ToTimeOnly(this DateTimeOffset date)
        => TimeOnly.FromTimeSpan(date.TimeOfDay);

    public static DateTimeOffset ToHungarianTime(this DateTimeOffset date)
        => date.ToOffset(GetHungarianOffset(date));

    /// <summary>
    /// Returns the Hungarian time zone offset compared to UTC at a given date time.
    /// </summary>
    public static TimeSpan GetHungarianOffset(DateTimeOffset dateTime) => _hungarianTimeZone.GetUtcOffset(dateTime);

    /// <summary>
    /// Converts the date to a DateTimeOffset at 0:00, in Hungarian TimeZone (considering DST)
    /// </summary>
    public static DateTimeOffset ToStartOfDay(this DateOnly date)
    {
        DateTime startLocal = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        TimeSpan startOffset = _hungarianTimeZone.GetUtcOffset(startLocal);
        return new DateTimeOffset(startLocal, startOffset).ToUniversalTime();
    }

    /// <summary>
    /// Converts the date to a DateTimeOffset at 23:59:59.9999999Z, in Hungarian TimeZone (considering DST)
    /// </summary>
    public static DateTimeOffset ToEndOfDay(this DateOnly date)
    {
        DateTime endLocal = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Unspecified);
        TimeSpan endOffset = _hungarianTimeZone.GetUtcOffset(endLocal);
        return new DateTimeOffset(endLocal, endOffset).ToUniversalTime();
    }

    public static DateTimeOffset ToUtc(this DateTime self, bool preferDst = false)
    {
        if (!_hungarianTimeZone.IsAmbiguousTime(self))
            return new DateTimeOffset(self, _hungarianTimeZone.GetUtcOffset(self)).ToUniversalTime();

        var offsets = _hungarianTimeZone
            .GetAmbiguousTimeOffsets(self)
            .OrderByDescending(o => o)
            .ToArray();

        var summer = offsets[0];
        var winter = offsets[1];

        return new DateTimeOffset(self, preferDst ? winter : summer).ToUniversalTime();
    }

    public static DateOnly LastDayOfMonth(this DateTimeOffset dateTime)
        => new(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
}
