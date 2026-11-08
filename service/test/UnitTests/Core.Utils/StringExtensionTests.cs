using FluentAssertions;
using Medusa.Enap.Common.Core.Utils;

namespace UnitTests.Core.Utils;
public class StringExtensionTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData("A", "a")]
    [InlineData("ab", "ab")]
    [InlineData("AB", "aB")]
    public void ToCamelCase(string input, string expected)
    {
        input.ToCamelCase().Should().Be(expected);
    }

    [Theory]
    [InlineData("2025-01-01", true)]
    [InlineData("2025.01.01", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("not_a_date", false)]
    public void TryParseDateOnly(string input, bool expected)
    {
        input.TryParseDateOnly(out var _).Should().Be(expected);
    }

    [Theory]
    [InlineData("2025-01-01 00:00:00", true)]
    [InlineData("2025-01-01T00:00:00+01", true)]
    [InlineData("2025.01.01 00:00:00+02", true)]
    [InlineData("2025-01-01 25:00:00", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("not_a_date", false)]
    public void TryParseDateTimeOffset(string input, bool expected)
    {
        input.TryParseDateTimeOffset(out var _).Should().Be(expected);
    }
}
