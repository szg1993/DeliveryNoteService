using FluentAssertions;
using Common.Core.Utils;
using Common.Testing.TestSuite;

namespace UnitTests.Core.Utils;

public class TimeRangeUtilTests
{
    public static TheoryData<DateOnly?, DateOnly?, bool> ConsecutivenessTestData => new()
    {
        { TestDates.Jan31, TestDates.Feb1, true},
        { TestDates.Feb1, TestDates.Jan31, false},
        { TestDates.Mar1, TestDates.Mar3, false},
        { TestDates.Mar3, TestDates.Mar1, false},
        { TestDates.Mar3, null, false},
        { null, TestDates.Mar3, false},
        { null, null, false},
    };

    [Theory]
    [MemberData(nameof(ConsecutivenessTestData))]
    public void AreConsecutive(DateOnly? d1, DateOnly? d2, bool success)
    {
        TimeRangeUtils.AreConsecutive(d1, d2).Should().Be(success);
    }
}