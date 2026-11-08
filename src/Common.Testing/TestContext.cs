using System.Reflection;
using Xunit.Abstractions;

namespace Medusa.Enap.Common.Testing;

public static class TestContext
{
    public static ITestCase? GetTestCase(this ITestOutputHelper output)
    {
        var type = output.GetType();
        var testMember = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
        var test = testMember?.GetValue(output) as ITest;
        return test?.TestCase;
    }
}
