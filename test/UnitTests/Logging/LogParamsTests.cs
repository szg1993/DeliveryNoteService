using Common.Core.Logging;
using FluentAssertions;

namespace UnitTests.Logging;

public class LogParamsTests
{
    public const string TestField = "TestValue";

    [Fact]
    public void Add()
    {
        var lp = new LogParams()
            .Add("key", "value")
            .Add("_someField", "fieldValue")
            .Add("someInstance.SomeProperty", "propertyValue")
            .Add("_NullValue", null)
            .AddParam(LogParamsTests.TestField);

        Dictionary<string, object?> dict = lp;

        dict.Should().BeEquivalentTo(new Dictionary<string, object?>()
        {
            { "key", "value" },
            { "someField", "fieldValue" },
            { "someProperty", "propertyValue" },
            { "nullValue", null},
            { "testField", "TestValue" },
        });
    }
}