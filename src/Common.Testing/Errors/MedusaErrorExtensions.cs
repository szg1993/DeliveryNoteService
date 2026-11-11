using Common.Core.Exceptions;
using Common.Core.Utils;
using FluentAssertions;

namespace Common.Testing.Errors;

public static class MedusaErrorExtensions
{
    public static Task<MedusaError> WithData(this Task<MedusaError> errorTask, string key, string value)
        => errorTask.Then(e => e.AssertData(key, value));

    public static MedusaError AssertData(this MedusaError error, string key, string value)
    {
        error.Data.Should().ContainKey(key);
        error.Data[key].Should().Be(value);
        return error;
    }
}