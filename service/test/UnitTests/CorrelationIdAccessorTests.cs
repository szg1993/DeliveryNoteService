using FluentAssertions;
using Common.Core.Integration;

namespace UnitTests;

public class CorrelationIdAccessorTests
{
    [Fact]
    public void Ctor_HasGuidCorrelationId()
    {
        var accessor = new CorrelationIdAccessor();

        Guid.TryParse(accessor.CorrelationId, out var parsedCorrId).Should().BeTrue();
        parsedCorrId.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void InvalidCorrelationIdIsOverwritten(string input)
    {
        var accessor = new CorrelationIdAccessor();
        var original = accessor.CorrelationId;

        accessor.CorrelationId = input;

        accessor.CorrelationId.Should().Be(original);
    }

    [Theory]
    [InlineData("test string value")]
    [InlineData("1561E163-8BA6-4F82-A37A-488B6F3CDC30")]
    public void ValidCorrelationIdIsKept(string input)
    {
        var accessor = new CorrelationIdAccessor();

        accessor.CorrelationId = input;

        accessor.CorrelationId.Should().Be(input);
    }
}