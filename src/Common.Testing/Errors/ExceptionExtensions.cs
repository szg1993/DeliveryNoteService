using FluentAssertions;

namespace Medusa.Enap.Common.Testing.Errors;

public static class ExceptionExtensions
{
    public static Exception AssertMaxLength(this Exception self, int maxLength)
        => AssertData(self, "MaxLength", maxLength);

    public static Exception AssertData(this Exception self, string key, object value)
    {
        self.Data[key].Should().Be(value);
        return self;
    }
}