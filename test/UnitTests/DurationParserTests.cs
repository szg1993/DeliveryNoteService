using Common.Core.Utils;
using FluentAssertions;

namespace UnitTests;

public class DurationParserTests
{
    [Fact]
    public void Parse_Basic()
    {
        var duration = "30s";

        TimeSpan parsed = DurationParser.Parse(duration);

        parsed.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void Parse_EmptyString()
    {
        var act = () => DurationParser.Parse(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Parse_WrongFormat()
    {
        var duration = "13asd";

        var act = () => DurationParser.Parse(duration);

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Parse_ComplexDuration()
    {
        var duration = "1s2d3h4m";

        var parsed = DurationParser.Parse(duration);

        parsed.Should().Be(
            TimeSpan.FromSeconds(1)
            + TimeSpan.FromDays(2)
            + TimeSpan.FromHours(3)
            + TimeSpan.FromMinutes(4));
    }
}