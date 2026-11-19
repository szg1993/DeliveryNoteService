using FluentAssertions;
using System.Globalization;

namespace UnitTests.Core.Utils;

public class DateTimeExtensionsTest
{
    [Fact]
    public void ToStartOfDay()
    {
        var date = new DateOnly(2025, 2, 12);

        var dt = date.ToStartOfDay();

        dt.Should().Be(new DateTimeOffset(2025, 2, 12, 0, 0, 0, TimeSpan.FromHours(1)));
        dt.ToString("o").Should().Be("2025-02-11T23:00:00.0000000+00:00");
        dt.UtcDateTime.ToString("o").Should().Be("2025-02-11T23:00:00.0000000Z");
    }

    [Fact]
    public void ToStartOfDay_DST_Change()
    {
        var date = new DateOnly(2025, 3, 30);

        var dt = date.ToStartOfDay();

        dt.Should().Be(new DateTimeOffset(2025, 3, 30, 0, 0, 0, TimeSpan.FromHours(1)));
        dt.ToString("o").Should().Be("2025-03-29T23:00:00.0000000+00:00");
        dt.UtcDateTime.ToString("o").Should().Be("2025-03-29T23:00:00.0000000Z");
    }

    [Fact]
    public void ToEndOfDay()
    {
        var date = new DateOnly(2025, 2, 12);

        var dt = date.ToEndOfDay();

        dt.Should().BeCloseTo(new DateTimeOffset(2025, 2, 12, 23, 59, 59, TimeSpan.FromHours(1)), precision: TimeSpan.FromSeconds(1));
        dt.ToString("o").Should().Be("2025-02-12T22:59:59.9999999+00:00");
        dt.UtcDateTime.ToString("o").Should().Be("2025-02-12T22:59:59.9999999Z");
    }

    [Fact]
    public void ToEndOfDay_DST_Change()
    {
        var date = new DateOnly(2025, 3, 30);

        var dt = date.ToEndOfDay();

        dt.Should().BeCloseTo(new DateTimeOffset(2025, 3, 30, 23, 59, 59, TimeSpan.FromHours(2)), precision: TimeSpan.FromSeconds(1));
        dt.ToString("o").Should().Be("2025-03-30T21:59:59.9999999+00:00");
        dt.UtcDateTime.ToString("o").Should().Be("2025-03-30T21:59:59.9999999Z");
    }

    [Theory]
    [InlineData("2026. 10. 25. 1:59:00", false, "2026. 10. 24. 23:59:00 +00:00")]
    [InlineData("2026. 10. 25. 1:59:00", true, "2026. 10. 24. 23:59:00 +00:00")]
    // DST overlap starts
    [InlineData("2026. 10. 25. 2:00:00", false, "2026. 10. 25. 0:00:00 +00:00")]
    [InlineData("2026. 10. 25. 2:00:00", true, "2026. 10. 25. 1:00:00 +00:00")]
    [InlineData("2026. 10. 25. 2:59:00", false, "2026. 10. 25. 0:59:00 +00:00")]
    [InlineData("2026. 10. 25. 2:59:00", true, "2026. 10. 25. 1:59:00 +00:00")]
    // DST overlap ends
    [InlineData("2026. 10. 25. 3:00:00", false, "2026. 10. 25. 2:00:00 +00:00")]
    [InlineData("2026. 10. 25. 3:00:00", true, "2026. 10. 25. 2:00:00 +00:00")]
    public void ToUtc_DST_Change(string dateTime, bool preferDst, string dateTimeOffset)
    {
        var parsedDateTime = DateTime.ParseExact(
            dateTime,
            "yyyy. MM. dd. H:mm:ss",
            CultureInfo.InvariantCulture
        );
        var expected = DateTimeOffset.ParseExact(
            dateTimeOffset,
            "yyyy. MM. dd. H:mm:ss zzz",
            CultureInfo.InvariantCulture
        );

        parsedDateTime.ToUtc(preferDst).Should().Be(expected);
    }

    [Theory]
    [InlineData(520, 0)]
    [InlineData(520, 987)]
    public void ToIso8601String(int millisecond, int microsecond)
    {
        var date = new DateTimeOffset(2025, 10, 25, 21, 42, 36, millisecond, microsecond, TimeSpan.FromHours(2));

        var result = date.ToIso8601String();

        result.Should().Be("2025-10-25T19:42:36.520Z");
    }
}
