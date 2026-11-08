using FluentAssertions;
using FluentAssertions.Equivalency;

namespace Medusa.Enap.Common.Testing;

public static class AssertionHelpers
{
    /// <summary>
    /// Allows 1 millisecond tolerance for DateTimeOffset comparisons
    /// </summary>
    public static Func<EquivalencyAssertionOptions<object>, EquivalencyAssertionOptions<object>> WithTimeTolerance()
        => options => options
            .Using<DateTimeOffset>(ctx =>
                ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(1)))
            .WhenTypeIs<DateTimeOffset>();
}
